using FluentValidation;
using System.ComponentModel.DataAnnotations;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public class InitWalletRequest(string Password)
    {
        /// <summary>
        /// The passsword of the wallet
        /// </summary>
        /// <example>Secret99$</example>
        [Required]
        [MinLength(8), MaxLength(255)]
        public string Password { get; } = Password;

        public class Validator : AbstractValidator<InitWalletRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Password).WalletPassword();
            }
        }
    }
}
