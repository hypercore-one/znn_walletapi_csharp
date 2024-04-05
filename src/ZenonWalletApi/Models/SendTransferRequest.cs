using FluentValidation;
using Zenon.Model.Primitives;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record SendTransferRequest(
        Address address,
        string amount,
        TokenStandard tokenStandard)
    {
        public class Validator : AbstractValidator<SendTransferRequest>
        {
            public Validator()
            {
                RuleFor(x => x.address).NotNull().NotEqual(Address.EmptyAddress);
                RuleFor(x => x.amount).WalletAmount();
                RuleFor(x => x.tokenStandard).NotNull().NotEqual(TokenStandard.EmptyZts);
            }
        }
    }
}
