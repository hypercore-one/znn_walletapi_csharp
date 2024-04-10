using FluentValidation;
using System.ComponentModel.DataAnnotations;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record RestoreWalletRequest
    {
        [Required]
        [MinLength(8), MaxLength(255)]
        public required string Password { get; set; }

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
