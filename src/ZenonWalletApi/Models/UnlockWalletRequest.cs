using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models
{
    public record UnlockWalletRequest
    {
        /// <summary>
        /// The password of the wallet
        /// </summary>
        /// <example>Secret99$</example>
        [Required]
        public required string Password { get; set; }

        public class Validator : AbstractValidator<UnlockWalletRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Password).NotEmpty();
            }
        }
    }
}
