using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Xml.Linq;
using Zenon.Client;
using Zenon.Model;
using Zenon.Model.NoM;
using Zenon.Model.Primitives;
using Zenon.Wallet;

namespace ZenonWalletApi.Services
{
    public interface IAutoReceiveService : IHostedService, IDisposable
    {
        Task SubscribeAsync(int accountIndex);

        Task UnsubscribeAsync(int accountIndex);
    }

    public class AutoReceiveOptions
    {
        public const string AutoReceiver = "Api:AutoReceiver";

        public bool Enabled { get; set; } = true;
    }

    public class AutoReceiveService : BackgroundService, IAutoReceiveService
    {
        public AutoReceiveService(ILogger<AutoReceiveService> logger, IOptions<AutoReceiveOptions> options, IWalletService wallet, INodeService node)
        {
            Options = options.Value;
            Logger = logger;
            Wallet = wallet;
            Node = node;
        }

        private ConcurrentDictionary<string, int> AddressMap { get; } = new ConcurrentDictionary<string, int>();
        private ConcurrentQueue<Hash> Queue { get; } = new ConcurrentQueue<Hash>();
        
        private ILogger<AutoReceiveService> Logger { get; }
        private AutoReceiveOptions Options { get; }
        private IWalletService Wallet { get; }
        private INodeService Node { get; }

        public async Task SubscribeAsync(int accountIndex)
        {
            var address = await Wallet.GetAccountAddressAsync(accountIndex);

            Logger.LogInformation($"Subscribe to: {address}");

            AddressMap.TryAdd(address.ToString(), accountIndex);

            await QueueUnreceivedTransactionsByAddressAsync(address);
        }

        public async Task UnsubscribeAsync(int accountIndex)
        {
            var address = await Wallet.GetAccountAddressAsync(accountIndex);

            Logger.LogInformation($"Unsubscribe from: {address}");

            AddressMap.Remove(address.ToString(), out _);
        }

        private async Task ListenToAllAccountBlocksAsync()
        {
            Logger.LogDebug("Listen to accounts blocks");

            Node.Subscribe("ledger.subscription", OnSubscriptionEvent);
            await Node.Api.Subscribe.ToAllAccountBlocks();
        }

        private void OnSubscriptionEvent(string subscription, JToken[] result)
        {
            foreach (var tx in result)
            {
                var toAddressString = tx.Value<string?>("toAddress");
                var hashString = tx.Value<string?>("hash");

                if (toAddressString != null &&
                    hashString != null &&
                    AddressMap.ContainsKey(toAddressString))
                {
                    _ = QueueTxAsync(Hash.Parse(hashString));
                }
            }
        }

        private async Task QueueUnreceivedTransactionsAsync()
        {
            foreach (var address in AddressMap.Keys)
            {
                await QueueUnreceivedTransactionsByAddressAsync(Address.Parse(address));
            }
        }

        private async Task QueueUnreceivedTransactionsByAddressAsync(Address address)
        {
            uint pageIndex = 0;
            var more = true;
            while (more)
            {
                var unreceivedBlocks = await Node.Api.Ledger.GetUnreceivedBlocksByAddress(address, pageIndex);

                if (unreceivedBlocks.List == null ||
                    unreceivedBlocks.List.Length == 0)
                    break;

                foreach (var unreceivedBlock in unreceivedBlocks.List)
                {
                    await QueueTxAsync(unreceivedBlock.Hash);
                }

                pageIndex += 1;
                more = unreceivedBlocks.More;
            }
        }

        private async Task QueueTxAsync(Hash blockHash)
        {
            if (!Queue.Contains(blockHash))
            {
                var syncInfo = await Node.Api.Stats.SyncInfo();

                if (syncInfo.state == (int)SyncState.SyncDone ||
                      (syncInfo.targetHeight > 0 &&
                          syncInfo.currentHeight > 0 &&
                          (syncInfo.targetHeight - syncInfo.currentHeight) < 3))
                {
                    Logger.LogDebug($"Queue block hash: {blockHash}");

                    Queue.Enqueue(blockHash);
                }
            }
        }

        private async Task ProcessQueueAsync()
        {
            while (Queue.TryPeek(out var blockHash))
            {
                try
                {
                    var toAddress =
                        (await Node.Api.Ledger.GetAccountBlockByHash(blockHash))!.ToAddress;

                    if (AddressMap.TryGetValue(toAddress.ToString(), out var accountIndex))
                    {
                        var account = await Wallet.GetAccountAsync(accountIndex);

                        Logger.LogInformation($"Receive block: {blockHash}");

                        var block = AccountBlockTemplate.Receive(Node.ProtocolVersion, Node.ChainIdentifier, blockHash);

                        await Node.SendAsync(block, account);
                    }

                    Queue.TryDequeue(out _);
                }
                catch (RemoteInvocationException e)
                {
                    Logger.LogWarning(e.Message);

                    if (e.Message.Contains("already received"))
                    {
                        Queue.TryDequeue(out _);
                    }
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var initialized = false;

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                if (Options.Enabled)
                {
                    try
                    {
                        if (Node.IsClosed)
                        {
                            Logger.LogDebug("Connecting");

                            initialized = false;

                            await Node.ConnectAsync(stoppingToken);
                        }
                        else if (!initialized)
                        {
                            Logger.LogDebug("Initializing");

                            await QueueUnreceivedTransactionsAsync();
                            await ListenToAllAccountBlocksAsync();

                            initialized = true;
                        }
                        else if (Wallet.IsUnlocked)
                        {
                            await ProcessQueueAsync();
                        }
                    }
                    catch (WebSocketException e)
                    {
                        Logger.LogWarning(e.Message);
                    }
                    catch (WalletException e)
                    {
                        Logger.LogWarning(e.Message);
                    }
                    catch (NoConnectionException)
                    {
                        Logger.LogWarning("No node connection");
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "An unexpected exception occurred");
                    }
                }
            }
        }
    }
}
