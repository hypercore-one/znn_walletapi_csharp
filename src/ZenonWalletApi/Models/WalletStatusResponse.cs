namespace ZenonWalletApi.Models
{
    public record WalletStatusResponse(bool IsInitialized, bool IsUnlocked)
    {
        /// <summary>
        /// Indicates whether the wallet is initialized
        /// </summary>
        public bool IsInitialized { get; } = IsInitialized;

        /// <summary>
        /// Indicates whether the wallet is unlocked
        /// </summary>
        public bool IsUnlocked { get; } = IsUnlocked;
    }
}
