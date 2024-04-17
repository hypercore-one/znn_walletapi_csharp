using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record class WalletAccountAddressResponse(Address Address)
    {
        /// <summary>
        /// The wallet account address
        /// </summary>
        /// <example>z1qqjnwjjpnue8xmmpanz6csze6tcmtzzdtfsww7</example>
        public Address Address { get; } = Address;
    }
}
