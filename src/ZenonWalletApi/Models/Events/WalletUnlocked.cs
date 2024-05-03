using NetLah.Extensions.EventAggregator;

namespace ZenonWalletApi.Models.Events
{
    public sealed class WalletUnlocked : IEvent
    {
        public required WalletAccount[] Accounts { get; set; }
    }
}
