﻿using Microsoft.Extensions.Options;
using NetLah.Extensions.EventAggregator;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Zenon.Client;
using Zenon.Model;
using Zenon.Model.NoM;
using Zenon.Model.Primitives;
using Zenon.Wallet;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Events;
using ZenonWalletApi.Options;

namespace ZenonWalletApi.Services
{
    public interface IAutoReceiverService : IHostedService,
        IAsyncSubscriber<WalletInitialized>,
        IAsyncSubscriber<WalletUnlocked>,
        IAsyncSubscriber<WalletAccountsAdded>
    {
        bool IsEnabled { get; }
    }

    internal class AutoReceiverService : BackgroundService,
        IAutoReceiverService, IDisposable
    {
        private readonly ConcurrentQueue<Hash> blockQueue = new ConcurrentQueue<Hash>();
        private readonly ConcurrentQueue<WalletAccount> accountQueue = new ConcurrentQueue<WalletAccount>();

        private readonly SemaphoreSlim accountLock = new SemaphoreSlim(1, 1);
        private readonly List<WalletAccount> accountList = new List<WalletAccount>();
        private volatile WalletAccount[] accountArray = new WalletAccount[0];

        public AutoReceiverService(
            ILogger<AutoReceiverService> logger,
            IOptions<AutoReceiverOptions> options,
            IWalletService wallet,
            INodeService node)
        {
            Logger = logger;
            Options = options.Value;
            Timer = new PeriodicTimer(Options.TimerInterval);
            Wallet = wallet;
            Node = node;
        }

        private ILogger<AutoReceiverService> Logger { get; }

        private AutoReceiverOptions Options { get; }

        private PeriodicTimer Timer { get; }

        private IWalletService Wallet { get; }

        private INodeService Node { get; }

        public bool IsEnabled => Options.Enabled;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var init = true;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (await Timer.WaitForNextTickAsync(stoppingToken) && IsEnabled)
                    {
                        if (Node.IsClosed)
                        {
                            Logger.LogDebug("Connect node");

                            init = await Node.ConnectAsync(stoppingToken);
                        }
                        else
                        {
                            if (init) // Init after connection loss
                            {
                                var synced = await GetSyncStatusAsync();

                                if (synced)
                                {
                                    // Resync all accounts when connection is lost
                                    await QueueUnreceivedBlocksAsync();
                                    await ListenToAllAccountBlocksAsync();

                                    init = false;
                                }
                            }
                            else
                            {
                                // Sync added accounts
                                if (!accountQueue.IsEmpty)
                                {
                                    Logger.LogDebug("Process account queue");

                                    await ProcessAccountQueueAsync();
                                }

                                if (!blockQueue.IsEmpty && Wallet.IsUnlocked)
                                {
                                    Logger.LogDebug("Process block queue");

                                    await ProcessBlockQueueAsync();
                                }
                            }
                        }
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
                    Logger.LogError(e, "An unexpected exception occurred while processing queue");
                }
            }
        }

        private async Task<bool> GetSyncStatusAsync()
        {
            var syncInfo = await Node.Api.Stats.SyncInfo();

            return (syncInfo.state == (int)SyncState.SyncDone ||
                (syncInfo.targetHeight > 0 &&
                syncInfo.currentHeight > 0 &&
                (syncInfo.targetHeight - syncInfo.currentHeight) < 3));
        }

        private async Task ListenToAllAccountBlocksAsync()
        {
            Logger.LogDebug("Listen to accounts blocks");

            Node.Subscribe("ledger.subscription", OnBlockEvent);

            await Node.Api.Subscribe.ToAllAccountBlocks();
        }

        private void OnBlockEvent(string subscription, JToken[] result)
        {
            foreach (var tx in result)
            {
                var toAddressString = tx.Value<string?>("toAddress");
                var hashString = tx.Value<string?>("hash");

                if (toAddressString == null || hashString == null)
                    continue;

                try
                {
                    var toAddress = Address.Parse(toAddressString);

                    if (HasAccount(toAddress))
                    {
                        QueueBlock(Hash.Parse(hashString));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to queue block");
                }
            }
        }

        private async Task QueueUnreceivedBlocksAsync()
        {
            var accounts = GetAccounts();

            foreach (var account in accounts)
            {
                await QueueUnreceivedBlocksByAddressAsync(account.Address);
            }
        }

        private async Task QueueUnreceivedBlocksByAddressAsync(Address address)
        {
            uint pageIndex = 0;
            var more = true;
            while (more)
            {
                var unreceivedBlocks = await Node.Api.Ledger.GetUnreceivedBlocksByAddress(address, pageIndex);

                if (unreceivedBlocks?.List == null ||
                    unreceivedBlocks.List.Length == 0)
                    break;

                foreach (var unreceivedBlock in unreceivedBlocks.List)
                {
                    QueueBlock(unreceivedBlock.Hash);
                }

                pageIndex += 1;
                more = unreceivedBlocks.More;
            }
        }

        private void QueueBlock(Hash blockHash)
        {
            if (!blockQueue.Contains(blockHash))
            {
                Logger.LogDebug($"Queue block: {blockHash}");

                blockQueue.Enqueue(blockHash);
            }
        }

        private async Task ProcessBlockQueueAsync()
        {
            while (blockQueue.TryPeek(out var blockHash))
            {
                try
                {
                    var toAddress =
                        (await Node.Api.Ledger.GetAccountBlockByHash(blockHash))!.ToAddress;

                    if (HasAccount(toAddress))
                    {
                        var account = await Wallet.GetAccountAsync(toAddress);

                        Logger.LogInformation($"Receive block: {blockHash}");

                        var block = AccountBlockTemplate.Receive(Node.ProtocolVersion, Node.ChainIdentifier, blockHash);

                        await Node.SendAsync(block, account);
                    }

                    blockQueue.TryDequeue(out _);
                }
                catch (RemoteInvocationException e)
                {
                    Logger.LogWarning(e.Message);

                    if (e.Message.Contains("already received"))
                    {
                        blockQueue.TryDequeue(out _);
                    }
                }
            }
        }

        private bool HasAccount(Address address)
        {
            return accountArray.FirstOrDefault(x => x.Address == address) != null;
        }

        private WalletAccount[] GetAccounts()
        {
            return accountArray;
        }

        private async Task ClearQueuesAndAccountsAsync()
        {
            Logger.LogDebug("Clear queues and accounts");

            await accountLock.WaitAsync();

            try
            {
                blockQueue.Clear();
                accountQueue.Clear();
                
                accountList.Clear();
                accountArray = accountList.ToArray();
            }
            finally
            {
                accountLock.Release();
            }
        }

        private void QueueAccount(WalletAccount account)
        {
            if (!accountQueue.Contains(account))
            {
                Logger.LogDebug($"Queue account: {account}");

                accountQueue.Enqueue(account);
            }
        }

        private async Task ProcessAccountQueueAsync()
        {
            var accountsToAdd = new List<WalletAccount>();

            try
            {
                while (accountQueue.TryPeek(out var account))
                {
                    if (!HasAccount(account.Address))
                    {
                        await QueueUnreceivedBlocksByAddressAsync(account.Address);

                        Logger.LogDebug($"Add account: {account}");

                        accountsToAdd.Add(account);
                    }

                    accountQueue.TryDequeue(out _);
                }
            }
            finally
            {
                if (accountsToAdd.Count > 0)
                {
                    await accountLock.WaitAsync();

                    try
                    {
                        accountList.AddRange(accountsToAdd);
                        accountArray = accountList.ToArray();
                    }
                    finally
                    {
                        accountLock.Release();
                    }
                }
            }
        }

        #region Event Handlers

        public async Task HandleAsync(WalletInitialized? @event, CancellationToken cancellationToken)
        {
            if (@event != null)
            {
                await ClearQueuesAndAccountsAsync();

                if (@event.Accounts != null)
                {
                    foreach (var account in @event.Accounts)
                    {
                        QueueAccount(account);
                    }
                }
            }
        }

        public async Task HandleAsync(WalletUnlocked? @event, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (@event != null && @event.Accounts != null)
                {
                    foreach (var account in @event.Accounts)
                    {
                        QueueAccount(account);
                    }
                }
            });
        }

        public async Task HandleAsync(WalletAccountsAdded? @event, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (@event != null && @event.Accounts != null)
                {
                    foreach (var account in @event.Accounts)
                    {
                        QueueAccount(account);
                    }
                }
            });
        }

        #endregion
    }
}
