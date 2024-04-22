using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record class WalletAccountList(WalletAccount[] List, int Count)
    {
        [Required]
        public WalletAccount[] List { get; set; } = List;

        [Required]
        public int Count { get; set; } = Count;
    }

    public class WalletAccount
    {
        public WalletAccount(Address address, int index)
        {
            Address = address;
            Index = index;
        }

        [Required]
        public Address Address { get; }

        [Required]
        public int Index { get; }
    }
}
