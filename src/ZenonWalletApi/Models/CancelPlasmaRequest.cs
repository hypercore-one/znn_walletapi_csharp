using FluentValidation;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record CancelPlasmaRequest(
        Hash idHash)
    {
        public class Validator : AbstractValidator<CancelPlasmaRequest>
        {
            public Validator()
            {
                RuleFor(x => x.idHash).NotNull().NotEqual(Hash.Empty);
            }
        }
    }
}
