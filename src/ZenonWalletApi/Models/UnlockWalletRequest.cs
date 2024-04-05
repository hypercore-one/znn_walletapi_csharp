using FluentValidation;

namespace ZenonWalletApi.Models
{
    public record UnlockWalletRequest(
        string password)
    {
        public class Validator : AbstractValidator<UnlockWalletRequest>
        {
            public Validator()
            {
                RuleFor(x => x.password).NotEmpty();
            }
        }
    }
}
