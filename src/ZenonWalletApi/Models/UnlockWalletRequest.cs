using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models
{
    public record UnlockWalletRequest(string Password)
    {
        /// <summary>
        /// The password of the wallet
        /// </summary>
        /// <example>Secret99$</example>
        [Required]
        public string Password { get; } = Password;

        public class Validator : AbstractValidator<UnlockWalletRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Password).NotEmpty();
            }
        }
    }
}
