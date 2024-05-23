using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record WalletAccount(Address Address, int Index)
    {
        /// <summary>
        /// The wallet account address
        /// </summary>
        /// <example>z1qqjnwjjpnue8xmmpanz6csze6tcmtzzdtfsww7</example>
        [Required]
        public Address Address { get; } = Address;

        /// <summary>
        /// The wallet account index
        /// </summary>
        /// <example>0</example>
        [Required]
        public int Index { get; } = Index;

        public override int GetHashCode()
        {
            return Index;
        }

        public override string ToString()
        {
            return $"{Index}:{Address}";
        }
    }
}
