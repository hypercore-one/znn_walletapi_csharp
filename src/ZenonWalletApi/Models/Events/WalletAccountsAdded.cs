using NetLah.Extensions.EventAggregator;

namespace ZenonWalletApi.Models.Events
{
    public sealed class WalletAccountsAdded : IEvent
    {
        public required WalletAccount[] Accounts { get; set; }
    }
}
