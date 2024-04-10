using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models
{
    public record AuthenticateRequest
    {
        [Required]
        public required string Username { get; set; }

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
