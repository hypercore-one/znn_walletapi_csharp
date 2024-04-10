using FluentValidation;
using System.ComponentModel.DataAnnotations;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record InitWalletRequest
    {
        [Required]
        [MinLength(8), MaxLength(255)]
        public required string Password { get; set; }

        public class Validator : AbstractValidator<InitWalletRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Password).WalletPassword();
            }
        }
    }
}
