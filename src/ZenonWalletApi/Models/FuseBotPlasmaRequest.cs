using FluentValidation;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record FuseBotPlasmaRequest(Address address)
    {
        public class Validator : AbstractValidator<FuseBotPlasmaRequest>
        {
            public Validator()
            {
                RuleFor(x => x.address).NotNull().NotEqual(Address.EmptyAddress);
            }
        }
    }
}
