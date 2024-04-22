using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models
{
    public record InitWalletResponse(string Mnemonic)
    {
        /// <summary>
        /// The mnemonic of the initialized wallet
        /// </summary>
        /// <example>route become dream access impulse price inform obtain engage ski believe awful absent pig thing vibrant possible exotic flee pepper marble rural fire fancy</example>
        [Required]
        public string Mnemonic { get; } = Mnemonic;
    }
}
