using FluentValidation;
using System.ComponentModel;

namespace ZenonWalletApi.Models
{
    public class AuthenticateRequest
    {
        [DefaultValue("admin")]
        public required string Username { get; set; }

        [DefaultValue("admin")]
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
