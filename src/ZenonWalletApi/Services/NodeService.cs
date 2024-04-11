using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Zenon;
using Zenon.Client;
using Zenon.Model;
using Zenon.Model.NoM;
using Zenon.Model.Primitives;
using Zenon.Pow;
using Zenon.Wallet;
using ZenonWalletApi.Options;

namespace ZenonWalletApi.Services
{
    public interface INodeService : IClient
    {
        Zdk Api { get; }

        bool IsClosed { get; }

        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

        Task<AccountBlockTemplate> SendAsync(AccountBlockTemplate block, IWalletAccount account);
    }

    internal class NodeService : WsClient, INodeService, IDisposable
    {
        public NodeService(ILogger<NodeService> logger, IOptions<NodeOptions> options, IAutoLockerService autoLocker)
            : this(logger, options.Value, autoLocker)
        { }

        public NodeService(ILogger<NodeService> logger, NodeOptions options, IAutoLockerService autoLocker)
           : base(options.NodeUrl, new WsClientOptions()
           {
               ChainIdentifier = options.ChainId,
               ProtocolVersion = options.ProtocolVersion
           })
        {
            Logger = logger;
            Options = options;
            PoW = new Semaphore(Options.MaxPoWThreads, Options.MaxPoWThreads);
            Api = new Zdk(this);
            AutoLocker = autoLocker;
        }

        private ConcurrentDictionary<Address, Semaphore> Semaphores { get; } =
            new ConcurrentDictionary<Address, Semaphore>();

        private ILogger Logger { get; }

        private NodeOptions Options { get; }

        private Semaphore PoW { get; }

        private IAutoLockerService AutoLocker { get; }

        public Zdk Api { get; }

        private Semaphore GetSemaphore(Address address)
        {
            return Semaphores.GetOrAdd(address, new Semaphore(1, 1));
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
                PoW.Release();

                Logger.LogInformation("Done");
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

        public async Task<AccountBlockTemplate> SendAsync(AccountBlockTemplate block, IWalletAccount account)
        {
            var syncInfo = await Api.Stats.SyncInfo();

            var nodeIsSynced = (syncInfo.state == (int)SyncState.SyncDone ||
                (syncInfo.targetHeight > 0 &&
                syncInfo.currentHeight > 0 &&
                (syncInfo.targetHeight - syncInfo.currentHeight) < 20));

            if (nodeIsSynced)
            {
                var address = await account.GetAddressAsync();
                var semaphore = GetSemaphore(address);
                try
                {
                    semaphore.WaitOne();

                    AutoLocker.Suspend();

                    var result = await Api.SendAsync(block, account, GeneratingPoW);

                    Logger.LogInformation($"Send block: {result.Hash}");

                    // Release the lock after 1 second.
                    //
                    // This will give the node enough time so that it'll process
                    // the transaction before we start creating a new one.
                    await Task.Delay(1000);

                    AutoLocker.Resume();

                    return result;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            else
            {
                throw new InvalidOperationException("Node is not synced");
            }
        }
    }
}
