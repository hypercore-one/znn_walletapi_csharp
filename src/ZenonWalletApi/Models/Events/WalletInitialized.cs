using NetLah.Extensions.EventAggregator;

namespace ZenonWalletApi.Models.Events
{
    public sealed class WalletInitialized : IEvent
    {
        public WalletAccount[]? Accounts { get; set; }
    }
}
