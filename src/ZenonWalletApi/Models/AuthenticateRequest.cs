using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models
{
    public record AuthenticateRequest(string Username, string Password)
    {
        /// <summary>
        /// The username of the user
        /// </summary>
        /// <example>user</example>
        [Required]
        public string Username { get; } = Username;

        /// <summary>
        /// The password of the user
        /// </summary>
        /// <example>user</example>
        [Required]
        public string Password { get; } = Password;

        public class Validator : AbstractValidator<AuthenticateRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Username).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
            }
        }
    }
}
