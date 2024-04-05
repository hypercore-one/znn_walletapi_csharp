using FluentValidation;
using Zenon.Model.Primitives;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record FusePlasmaRequest(Address address, string amount)
    {
        public class Validator : AbstractValidator<FusePlasmaRequest>
        {
            public Validator()
            {
                RuleFor(x => x.address).NotNull().NotEqual(Address.EmptyAddress);
                RuleFor(x => x.amount).WalletAmount();
            }
        }
    }
}
