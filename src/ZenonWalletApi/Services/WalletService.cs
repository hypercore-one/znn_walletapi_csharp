using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Zenon.Model.Primitives;
using Zenon.Wallet;
using Zenon.Wallet.Json;
using ZenonWalletApi.Models;
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

        Task<IWalletAccount> GetAccountAsync(Address address);

        Task<IWalletAccount> GetAccountAsync(int accountIndex);

        Task<int> GetAccountIndexAsync(Address accountIndex);

        Task<WalletAccountList> AddAccountsAsync(int numberOfAccounts);

        Task<WalletAccountList> GetAccountsAsync(int pageIndex, int pageSize);
    }

    internal class WalletService : BackgroundService, IWalletService, IDisposable
    {
        private const string AccountCountKey = "walletApi__accountCount";

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public WalletService(ILogger<WalletService> logger, IOptions<WalletOptions> options, IAutoLockerService autoLocker)
        {
            Logger = logger;
            Options = options.Value;
            WalletManager = new KeyStoreManager(Options.Path);
            AutoLocker = autoLocker;
        }

        private ILogger Logger { get; }

        private WalletOptions Options { get; }

        private IAutoLockerService AutoLocker { get; }

        private KeyStoreManager WalletManager { get; }

        private KeyStoreDefinition? WalletDefinition { get; set; }

        private KeyStore? Wallet { get; set; }

        private List<WalletAccount>? Accounts { get; set; }

        private int NumFailedUnlockAttempts { get; set; }

        public bool IsInitialized => WalletDefinition != null;

        public bool IsUnlocked => Wallet != null;

        public async Task<string> InitAsync(string password)
        {
            try
            {
                await _lock.WaitAsync();

                // Initialize a new wallet
                var walletDefinition = WalletManager
                .CreateNew(password, Options.Name);

                AutoLocker.Activity();

                // Unlock the wallet
                var wallet = (KeyStore)await WalletManager.GetWalletAsync(walletDefinition,
                    new KeyStoreOptions() { DecryptionPassword = password });

                WalletDefinition = walletDefinition;
                Wallet = wallet;
                NumFailedUnlockAttempts = 0;

                InitAccounts(1);

                Logger.LogInformation($"Initialize: {Options.Name}");

                return wallet!.Mnemonic;
            }
            finally
            {
                _lock.Release();
            } 
        }

        public async Task RestoreAsync(string password, string mnemonic)
        {
            try
            {
                await _lock.WaitAsync();

                // Initialize an existing wallet
                var walletDefinition = WalletManager
                    .CreateFromMnemonic(mnemonic, password, Options.Name);

                AutoLocker.Activity();

                // Unlock the wallet
                var wallet = (KeyStore)await WalletManager.GetWalletAsync(walletDefinition,
                    new KeyStoreOptions() { DecryptionPassword = password });

                WalletDefinition = walletDefinition;
                Wallet = wallet;
                NumFailedUnlockAttempts = 0;

                InitAccounts(1);

                Logger.LogInformation($"Restore: {Options.Name}");
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UnlockAsync(string password)
        {
            AssertInitialized();

            try
            {
                await _lock.WaitAsync();

                AutoLocker.Activity();

                var wallet = await WalletManager.GetWalletAsync(WalletDefinition,
                    new KeyStoreOptions() { DecryptionPassword = password }) as KeyStore;

                Wallet = wallet;
                NumFailedUnlockAttempts = 0;

                if (Accounts == null)
                {
                    var addressCount = ReadAccountCount(WalletDefinition!.WalletId);

                    if (addressCount == -1)
                        addressCount = 1; // Add base address by default

                    InitAccounts(addressCount);
                }

                Logger.LogInformation($"Unlock: {Options.Name}");
            }
            catch (IncorrectPasswordException)
            {
                if (Options.EraseLimit.HasValue)
                {
                    NumFailedUnlockAttempts += 1;

                    if (NumFailedUnlockAttempts >= Options.EraseLimit)
                    {
                        WalletDefinition = null;
                        Wallet = null;
                        NumFailedUnlockAttempts = 0;
                    }
                }

                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task LockAsync()
        {
            try
            {
                await _lock.WaitAsync();

                AutoLocker.Activity();

                Wallet = null;
                NumFailedUnlockAttempts = 0;

                Logger.LogInformation($"Lock: {Options.Name}");
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IWalletAccount> GetAccountAsync(Address address)
        {
            AssertInitialized();
            AssertUnlocked();

            var result = Accounts!.FirstOrDefault(x => x.Address == address);
            if (result != null)
                return await GetAccountAsync(result.Index);
            throw new WalletException("Account does not exist");
        }

        public async Task<IWalletAccount> GetAccountAsync(int accountIndex)
        {
            var wallet = GetWallet();
            var result = Accounts!.FirstOrDefault(x => x.Index == accountIndex);
            if (result != null)
                return await wallet.GetAccountAsync(result.Index);
            throw new WalletException("Account does not exist");
        }

        public async Task<int> GetAccountIndexAsync(Address address)
        {
            return await Task.Run(() =>
            {
                AssertInitialized();
                AssertUnlocked();

                var result = Accounts?.FirstOrDefault(x => x.Address == address);
                if (result != null)
                    return result.Index;
                throw new WalletException("Account does not exist");
            });
        }

        public async Task<WalletAccountList> GetAccountsAsync(int pageIndex, int pageSize)
        {
            return await Task.Run(() =>
            {
                AssertInitialized();
                AssertUnlocked();

                var pagedItems = Accounts!.Skip(pageIndex * pageSize).Take(pageSize);

                return new WalletAccountList(pagedItems.ToArray(), Accounts!.Count);
            });
        }

        public async Task<WalletAccountList> AddAccountsAsync(int numberOfAccounts)
        {
            if (numberOfAccounts < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfAccounts), numberOfAccounts, "must be bigger than 0");

            var accountsToAdd = new List<WalletAccount>();

            await _lock.WaitAsync();
            try
            {
                var wallet = GetWallet();
                

                var lastIndex = Accounts!.Count - 1;

                for (int i = 0; i < numberOfAccounts; i++)
                {
                    var account = await wallet.GetAccountAsync(++lastIndex);
                    var address = await account.GetAddressAsync();

                    accountsToAdd.Add(new WalletAccount(address, lastIndex));
                }

                WriteAccountCount(WalletDefinition!.WalletId, Accounts.Count + accountsToAdd.Count);

                Accounts.AddRange(accountsToAdd);
            }
            finally
            {
                _lock.Release();
            }

            return new WalletAccountList(accountsToAdd.ToArray(), Accounts.Count);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Try to find initialized wallet
            var walletDefinition = await GetWalletDefinitionAsync(Options.Name);

            if (walletDefinition != null)
            {
                try
                {
                    await _lock.WaitAsync();
                    WalletDefinition = walletDefinition;
                    NumFailedUnlockAttempts = 0;
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        private void AssertInitialized()
        {
            if (!IsInitialized)
                throw new WalletException("Wallet is not initialized");
        }

        private void AssertUnlocked()
        {
            if (!IsUnlocked)
                throw new WalletException("Wallet is locked");
        }

        private async Task<KeyStoreDefinition?> GetWalletDefinitionAsync(string walletName)
        {
            return (await WalletManager.GetWalletDefinitionsAsync())
                .OfType<KeyStoreDefinition>()
                .FirstOrDefault(x => string.Equals(x.WalletName, walletName, StringComparison.OrdinalIgnoreCase));
        }

        private KeyStore GetWallet()
        {
            AssertInitialized();
            AssertUnlocked();

            AutoLocker.Activity();

            return Wallet!;
        }

        private void InitAccounts(int numberOfAccounts)
        {
            var list = new List<WalletAccount>();

            for (int i = 0; i < numberOfAccounts; i++)
            {
                var address = Wallet!.GetKeyPair(i).Address;
                list.Add(new WalletAccount(address, i));
            }

            WriteAccountCount(WalletDefinition!.WalletId, list.Count());

            Accounts = list;
        }

        private int ReadAccountCount(string filePath)
        {
            try
            {
                return (int)ReadWalletMetadataValue(filePath, AccountCountKey);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to read account count from wallet metadata.");
            }

            return -1;
        }

        private void WriteAccountCount(string filePath, int value)
        {
            try
            {
                WriteWalletMetadataValue(filePath, AccountCountKey, value);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to write account count to wallet metadata.");
            }
        }

        private dynamic? ReadWalletMetadataValue(string filePath, string key)
        {
            var content = File.ReadAllText(filePath);
            var encrypted = new EncryptedFile(JEncryptedFile.FromJObject(JObject.Parse(content)));

            if (encrypted.Metadata != null &&
                encrypted.Metadata![key] != null)
            {
                return encrypted.Metadata![key];
            }

            return default;
        }

        private void WriteWalletMetadataValue(string filePath, string key, dynamic? value)
        {
            var content = File.ReadAllText(filePath);
            var encrypted = new EncryptedFile(JEncryptedFile.FromJObject(JObject.Parse(content)));

            if (encrypted.Metadata != null)
            {
                encrypted.Metadata![key] = value;
            }

            File.WriteAllText(filePath, encrypted.ToString());
        }
    }
}
