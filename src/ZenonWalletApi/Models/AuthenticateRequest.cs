using FluentValidation;

namespace ZenonWalletApi.Models
{
    public record AuthenticateRequest(string username = "admin", string password = "admin")
    {
        public class Validator : AbstractValidator<AuthenticateRequest>
        {
            public Validator()
            {
                RuleFor(x => x.username).NotEmpty();
                RuleFor(x => x.password).NotEmpty();
            }
        }
    }
}
