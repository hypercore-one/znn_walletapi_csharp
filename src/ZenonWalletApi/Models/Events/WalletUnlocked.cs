using NetLah.Extensions.EventAggregator;

namespace ZenonWalletApi.Models.Events
{
    public sealed class WalletUnlocked : IEvent
    {
        public WalletAccount[]? Accounts { get; set; }
    }
}
