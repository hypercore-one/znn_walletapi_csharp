using FluentValidation;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record InitWalletRequest(
        string password)
    {
        public class Validator : AbstractValidator<InitWalletRequest>
        {
            public Validator()
            {
                RuleFor(x => x.password).WalletPassword();
            }
        }
    }
}
