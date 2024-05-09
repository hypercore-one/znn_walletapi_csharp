using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Numerics;
using Zenon;
using Zenon.Client;
using Zenon.Model;
using Zenon.Model.NoM;
using Zenon.Model.Primitives;
using Zenon.Pow;
using Zenon.Wallet;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Exceptions;
using ZenonWalletApi.Options;

namespace ZenonWalletApi.Services
{
    public interface INodeService : IClient
    {
        Zdk Api { get; }

        bool IsClosed { get; }

        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

        Task<AccountBlockTemplate> SendAsync(AccountBlockTemplate block, IWalletAccount account, CancellationToken cancellationToken = default);
    }

    internal class NodeService : WsClient, INodeService, IDisposable
    {
        private readonly ConcurrentDictionary<Address, Semaphore> semaphores =
            new ConcurrentDictionary<Address, Semaphore>();

        public NodeService(ILogger<NodeService> logger, IOptions<NodeOptions> options, IAutoLockerService autoLocker, IPlasmaBotService plasmaBot)
            : this(logger, options.Value, autoLocker, plasmaBot)
        { }

        public NodeService(ILogger<NodeService> logger, NodeOptions options, IAutoLockerService autoLocker, IPlasmaBotService plasmaBot)
           : base(options.NodeUrl, new WsClientOptions()
           {
               ChainIdentifier = options.ChainId,
               ProtocolVersion = options.ProtocolVersion
           })
        {
            Logger = logger;
            Options = options;
            MinQsrThreshold = new BigInteger(Options.MinQsrThreshold) * 100000000;
            PoW = new Semaphore(Options.MaxPoWThreads, Options.MaxPoWThreads);
            Api = new Zdk(this);
            AutoLocker = autoLocker;
            PlasmaBot = plasmaBot;
        }

        private ILogger Logger { get; }

        private NodeOptions Options { get; }

        private Semaphore PoW { get; }

        private IAutoLockerService AutoLocker { get; }

        private IPlasmaBotService PlasmaBot { get; }

        private BigInteger MinQsrThreshold { get; }

        public Zdk Api { get; }

        private Semaphore GetSemaphore(Address address)
        {
            return semaphores.GetOrAdd(address, new Semaphore(1, 1));
        }

        private void GeneratingPoW(PowStatus status)
        {
            if (status == PowStatus.Generating)
            {
                PoW.WaitOne();

                Logger.LogInformation("Generating PoW");
            }
            else if (status == PowStatus.Done)
            {
                Logger.LogInformation("PoW done");

                PoW.Release();
            }
        }

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await base.ConnectAsync(false, cancellationToken);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Failed to connect: {e.Message}");
                return false;
            }
        }

        private async Task<bool> GetSyncStatusAsync()
        {
            var syncInfo = await Api.Stats.SyncInfo();

            return (syncInfo.state == (int)SyncState.SyncDone ||
                (syncInfo.targetHeight > 0 &&
                syncInfo.currentHeight > 0 &&
                (syncInfo.targetHeight - syncInfo.currentHeight) < 20));
        }

        public async Task<AccountBlockTemplate> SendAsync(AccountBlockTemplate block, IWalletAccount account, CancellationToken cancellationToken = default)
        {
            if (!await GetSyncStatusAsync())
                throw new InvalidOperationException("Node is not synced");

            var address = await account.GetAddressAsync();

            // lock address
            var semaphore = GetSemaphore(address);
            semaphore.WaitOne();

            try
            {
                // prevent auto-locker from locking wallet
                AutoLocker.Suspend();

                // auto fuse plasma when necessary
                if (Options.PlasmaMode != PlasmaMode.PoW)
                {
                    Logger.LogDebug($"Checking plasma for address: {address}");

                    var plasmaInfo = await Api.Embedded.Plasma.Get(address);

                    // auto fuse when minimum qsr threshold is not reached
                    if (plasmaInfo.QsrAmount < MinQsrThreshold)
                    {
                        var fuse = false;

                        if (plasmaInfo.QsrAmount == 0) // no qsr fused
                        {
                            fuse = true;
                        }
                        else
                        {
                            var expiry = await PlasmaBot.GetExpirationAsync(address);

                            // already fused
                            if (expiry.HasValue)
                            {
                                Logger.LogWarning($"Cannot fuse twice for address: {address}");
                            }
                            else
                            {
                                fuse = true;
                            }
                        }

                        if (fuse)
                        {
                            await PlasmaBot.FuseAsync(address);

                            var timeout = DateTime.UtcNow + Options.FuseTimeout;
                            var initialQsrAmount = plasmaInfo.QsrAmount;

                            // wait for timeout or fusion to complete
                            while (timeout > DateTime.UtcNow &&
                                !cancellationToken.IsCancellationRequested)
                            {
                                await Task.Delay(5000, cancellationToken);

                                plasmaInfo = await Api.Embedded.Plasma.Get(address);

                                // fused qsr has increased
                                if (plasmaInfo.QsrAmount > initialQsrAmount)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (plasmaInfo.QsrAmount < MinQsrThreshold)
                    {
                        if (Options.PlasmaMode == PlasmaMode.Fuse)
                            throw new NotFoundException("Not enough plasma");

                        Logger.LogWarning($"Minimum QSR threshold not reached for address: {address}");
                    }
                }

                var result = await Api.SendAsync(block, account, GeneratingPoW);

                Logger.LogInformation($"Send block: {result.Hash}");

                // Release the lock after 1 second.
                //
                // This will give the node enough time so that it'll process
                // the transaction before we start creating a new one.
                await Task.Delay(1000, cancellationToken);

                return result;
            }
            finally
            {
                AutoLocker.Resume();

                semaphore.Release();
            }
        }
    }
}
