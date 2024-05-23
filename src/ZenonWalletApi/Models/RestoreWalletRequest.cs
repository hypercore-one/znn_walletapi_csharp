using FluentValidation;
using System.ComponentModel.DataAnnotations;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record RestoreWalletRequest(string Password, string Mnemonic)
    {
        /// <summary>
        /// The password of the wallet
        /// </summary>
        /// <example>Secret99$</example>
        [Required]
        [MinLength(8), MaxLength(255)]
        public string Password { get; } = Password;

        /// <summary>
        /// The mnemonic of the wallet
        /// </summary>
        /// <example>route become dream access impulse price inform obtain engage ski believe awful absent pig thing vibrant possible exotic flee pepper marble rural fire fancy</example>
        [Required]
        public string Mnemonic { get; } = Mnemonic;

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
