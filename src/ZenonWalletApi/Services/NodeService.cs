using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Zenon;
using Zenon.Client;
using Zenon.Model;
using Zenon.Model.NoM;
using Zenon.Model.Primitives;
using Zenon.Pow;
using Zenon.Wallet;

namespace ZenonWalletApi.Services
{
    public interface INodeService : IClient, IDisposable
    {
        Zdk Api { get; }

        bool IsClosed { get; }

        Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

        Task<AccountBlockTemplate> SendAsync(AccountBlockTemplate block, IWalletAccount account);
    }

    public class NodeOptions
    {
        public const string Node = "Api:Node";

        [Required]
        public string NodeUrl { get; set; } = "ws://127.0.0.1:35998";
        [Required]
        public int ChainId { get; set; } = Constants.ChainId;
        [Required]
        public int ProtocolVersion { get; set; } = Constants.ProtocolVersion;

        public int MaxPoWThreads { get; set; } = 5;
    }

    public class NodeService : WsClient, INodeService
    {
        public NodeService(ILogger<NodeService> logger, IOptions<NodeOptions> options, IAutoLockService autoLocker)
            : base(options.Value.NodeUrl, new WsClientOptions()
            {
                ChainIdentifier = options.Value.ChainId,
                ProtocolVersion = options.Value.ProtocolVersion
            })
        {
            Logger = logger;
            Options = options.Value;
            PoW = new Semaphore(Options.MaxPoWThreads, Options.MaxPoWThreads);
            Api = new Zdk(this);
            AutoLocker = autoLocker;
        }

        private ConcurrentDictionary<Address, Semaphore> Semaphores { get; } =
            new ConcurrentDictionary<Address, Semaphore>();

        private ILogger Logger { get; }

        private NodeOptions Options { get; }

        private Semaphore PoW { get; }

        private IAutoLockService AutoLocker { get; }

        public Zdk Api { get; }

        public new bool IsClosed => base.IsClosed;

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
