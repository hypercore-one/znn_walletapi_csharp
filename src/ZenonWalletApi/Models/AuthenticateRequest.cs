using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models
{
    public record AuthenticateRequest
    {
        /// <summary>
        /// The username of the user
        /// </summary>
        /// <example>user</example>
        [Required]
        public required string Username { get; set; }

        /// <summary>
        /// The password of the user
        /// </summary>
        /// <example>user</example>
        [Required]
        public required string Password { get; set; }

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
