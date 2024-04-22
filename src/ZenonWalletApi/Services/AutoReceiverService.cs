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
using ZenonWalletApi.Options;

namespace ZenonWalletApi.Services
{
    public interface IAutoReceiverService : IHostedService
    {
        bool IsEnabled { get; }

        Task SubscribeAsync(Address accountAddress);

        Task SubscribeAsync(int accountIndex);

        Task UnsubscribeAsync(Address accountAddress);

        Task UnsubscribeAsync(int accountIndex);
    }

    internal class AutoReceiverService : BackgroundService, IAutoReceiverService, IDisposable
    {
        public AutoReceiverService(ILogger<AutoReceiverService> logger, IOptions<AutoReceiverOptions> options, IWalletService wallet, INodeService node)
        {
            Logger = logger;
            Options = options.Value;
            Timer = new PeriodicTimer(Options.TimerInterval);
            Wallet = wallet;
            Node = node;
        }

        private ConcurrentDictionary<string, int> AddressMap { get; } = new ConcurrentDictionary<string, int>();
        private ConcurrentQueue<Hash> Queue { get; } = new ConcurrentQueue<Hash>();

        private ILogger<AutoReceiverService> Logger { get; }

        private AutoReceiverOptions Options { get; }

        private PeriodicTimer Timer { get; }

        private IWalletService Wallet { get; }

        private INodeService Node { get; }

        public bool IsEnabled => Options.Enabled;

        public async Task SubscribeAsync(Address accountAddress)
        {
            var accountIndex = await Wallet.GetAccountIndexAsync(accountAddress);

            Logger.LogInformation($"Subscribe to: {accountAddress}");

            AddressMap.TryAdd(accountAddress.ToString(), accountIndex);

            await QueueUnreceivedTransactionsByAddressAsync(accountAddress);
        }

        public async Task SubscribeAsync(int accountIndex)
        {
            var account = await Wallet.GetAccountAsync(accountIndex);
            var accountAddress = await account.GetAddressAsync();

            Logger.LogInformation($"Subscribe to: {accountAddress}");

            AddressMap.TryAdd(accountAddress.ToString(), accountIndex);

            await QueueUnreceivedTransactionsByAddressAsync(accountAddress);
        }

        public async Task UnsubscribeAsync(Address address)
        {
            await Task.Run(() =>
            {
                if (AddressMap.Remove(address.ToString(), out _))
                {
                    Logger.LogInformation($"Unsubscribe from: {address}");
                }
            });
        }

        public async Task UnsubscribeAsync(int accountIndex)
        {
            await Task.Run(() =>
            {
                var address = AddressMap
                    .Where(x => x.Value == accountIndex)
                    .Select(x => x.Key)
                    .FirstOrDefault();

                if (address != null)
                {
                    if (AddressMap.Remove(address.ToString(), out _))
                    {
                        Logger.LogInformation($"Unsubscribe from: {address}");
                    }
                }
            });
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
                await Timer.WaitForNextTickAsync(stoppingToken);

                if (IsEnabled)
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
