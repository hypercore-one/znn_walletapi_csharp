using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record class WalletAccountAddressList(WalletAccountAddress[] List, int Count)
    {
        [Required]
        public WalletAccountAddress[] List { get; set; } = List;

        [Required]
        public int Count { get; set; } = Count;
    }

    public class WalletAccountAddress
    {
        public required Address Address { get; set; }
        public int AccountIndex { get; set; }
    }
}
