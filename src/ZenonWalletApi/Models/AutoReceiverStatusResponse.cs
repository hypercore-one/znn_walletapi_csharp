namespace ZenonWalletApi.Models
{
    public record AutoReceiverStatusResponse(bool IsEnabled)
    {
        /// <summary>
        /// Indicates whether the auto-receiver is enabled
        /// </summary>
        public bool IsEnabled { get; } = IsEnabled;
    }
}
