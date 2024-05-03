using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models
{
    public record WalletAccountList(WalletAccount[] List, int Count)
    {
        /// <summary>
        /// Gets a list of wallet accounts
        /// </summary>
        [Required]
        public WalletAccount[] List { get; } = List;

        /// <summary>
        /// Gets the total number of accounts
        /// </summary>
        [Required]
        public int Count { get; } = Count;
    }
}
