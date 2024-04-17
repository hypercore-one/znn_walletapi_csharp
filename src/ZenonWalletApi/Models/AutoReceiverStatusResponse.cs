namespace ZenonWalletApi.Models
{
    public record class AutoReceiverStatusResponse(bool IsEnabled)
    {
        /// <summary>
        /// Indicates whether the auto-receiver is enabled
        /// </summary>
        public bool IsEnabled { get; } = IsEnabled;
    }
}
