using FluentValidation;
using System.ComponentModel.DataAnnotations;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record RestoreWalletRequest
    {
        /// <summary>
        /// The password of the wallet
        /// </summary>
        /// <example>Secret99$</example>
        [Required]
        [MinLength(8), MaxLength(255)]
        public required string Password { get; set; }

        /// <summary>
        /// The mnemonic of the wallet
        /// </summary>
        /// <example>route become dream access impulse price inform obtain engage ski believe awful absent pig thing vibrant possible exotic flee pepper marble rural fire fancy</example>
        [Required]
        public required string Mnemonic { get; set; }

        public class Validator : AbstractValidator<RestoreWalletRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Password)
                    .WalletPassword();
                RuleFor(x => x.Mnemonic)
                    .NotEmpty()
                    .Must(value => Zenon.Wallet.Mnemonic.ValidateMnemonic(value));
            }
        }
    }
}
