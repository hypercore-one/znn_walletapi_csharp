using Microsoft.Extensions.Options;
using Zenon.Model.Primitives;
using Zenon.Wallet;
using ZenonWalletApi.Options;

namespace ZenonWalletApi.Services
{
    public interface IWalletService : IHostedService
    {
        bool IsInitialized { get; }

        bool IsUnlocked { get; }

        Task<string> InitAsync(string password);

        Task RestoreAsync(string password, string mnemonic);

        Task UnlockAsync(string password);

        Task LockAsync();

        Task<IWalletAccount> GetAccountAsync(int accountIndex);

        Task<Address> GetAccountAddressAsync(int accountIndex);
    }

    internal class WalletService : BackgroundService, IWalletService, IDisposable
    {
        public WalletService(ILogger<WalletService> logger, IOptions<WalletOptions> options, IAutoLockerService autoLocker)
        {
            Logger = logger;
            Options = options.Value;
            WalletManager = new KeyStoreManager(Options.Path);
            AutoLocker = autoLocker;
        }

        private object SyncLock { get; } = new object();

        private ILogger Logger { get; }

        private WalletOptions Options { get; }

        private IAutoLockerService AutoLocker { get; }

        private KeyStoreManager WalletManager { get; }

        private KeyStoreDefinition? WalletDefinition { get; set; }

        private KeyStore? Wallet { get; set; }

        private int NumFailedUnlockAttempts { get; set; }

        public bool IsInitialized => WalletDefinition != null;

        public bool IsUnlocked => Wallet != null;

        private async Task<KeyStoreDefinition?> GetWalletDefinitionAsync(string walletName)
        {
            return (await WalletManager.GetWalletDefinitionsAsync())
                .OfType<KeyStoreDefinition>()
                .FirstOrDefault(x => string.Equals(x.WalletName, walletName, StringComparison.OrdinalIgnoreCase));
        }

        private KeyStore GetWallet()
        {
            lock (SyncLock)
            {
                if (!IsInitialized)
                    throw new WalletException("Wallet is not initialized");

                if (!IsUnlocked)
                    throw new WalletException("Wallet is locked");

                AutoLocker.Activity();

                return Wallet!;
            }
        }

        public async Task<string> InitAsync(string password)
        {
            // Initialize a new wallet
            var walletDefinition = WalletManager
                .CreateNew(password, Options.Name);

            AutoLocker.Activity();

            // Unlock the wallet
            var wallet = await WalletManager.GetWalletAsync(walletDefinition,
                new KeyStoreOptions() { DecryptionPassword = password }) as KeyStore;

            lock (SyncLock)
            {
                WalletDefinition = walletDefinition;
                Wallet = wallet;
                NumFailedUnlockAttempts = 0;
            }

            Logger.LogInformation($"Initialize: {Options.Name}");

            return wallet!.Mnemonic;
        }

        public async Task RestoreAsync(string password, string mnemonic)
        {
            // Initialize an existing wallet
            var walletDefinition = WalletManager
               .CreateFromMnemonic(mnemonic, password, Options.Name);

            AutoLocker.Activity();

            // Unlock the wallet
            var wallet = await WalletManager.GetWalletAsync(walletDefinition,
                new KeyStoreOptions() { DecryptionPassword = password }) as KeyStore;

            lock (SyncLock)
            {
                WalletDefinition = walletDefinition;
                Wallet = wallet;
                NumFailedUnlockAttempts = 0;
            }

            Logger.LogInformation($"Restore: {Options.Name}");
        }

        public async Task UnlockAsync(string password)
        {
            if (!IsInitialized)
                throw new WalletException("Wallet is not initialized");

            AutoLocker.Activity();

            try
            {
                var wallet = await WalletManager.GetWalletAsync(WalletDefinition,
                    new KeyStoreOptions() { DecryptionPassword = password }) as KeyStore;

                lock (SyncLock)
                {
                    Wallet = wallet;
                    NumFailedUnlockAttempts = 0;
                }

                Logger.LogInformation($"Unlock: {Options.Name}");
            }
            catch (IncorrectPasswordException)
            {
                if (Options.EraseLimit.HasValue)
                {
                    lock (SyncLock)
                    {
                        NumFailedUnlockAttempts += 1;

                        if (NumFailedUnlockAttempts >= Options.EraseLimit)
                        {
                            WalletDefinition = null;
                            Wallet = null;
                            NumFailedUnlockAttempts = 0;
                        }
                    }
                }

                throw;
            }
        }

        public async Task LockAsync()
        {
            await Task.Run(() =>
            {
                AutoLocker.Activity();

                lock (SyncLock)
                {
                    Wallet = null;
                    NumFailedUnlockAttempts = 0;
                }

                Logger.LogInformation($"Lock: {Options.Name}");
            });
        }

        public async Task<IWalletAccount> GetAccountAsync(int accountIndex) =>
            await GetWallet().GetAccountAsync(accountIndex);

        public async Task<Address> GetAccountAddressAsync(int accountIndex) =>
           await (await GetWallet().GetAccountAsync(accountIndex)).GetAddressAsync();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Try to initialize wallet
            var walletDefinition = await GetWalletDefinitionAsync(Options.Name);

            lock (SyncLock)
            {
                WalletDefinition = walletDefinition;
                NumFailedUnlockAttempts = 0;
            }
        }
    }
}
