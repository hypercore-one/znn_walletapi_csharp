using FluentValidation;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record ReceiveTransferRequest(
        Hash blockHash)
    {
        public class Validator : AbstractValidator<ReceiveTransferRequest>
        {
            public Validator()
            {
                RuleFor(x => x.blockHash).NotNull().NotEqual(Hash.Empty);
            }
        }
    }
}
