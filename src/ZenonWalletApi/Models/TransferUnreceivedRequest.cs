using FluentValidation;

namespace ZenonWalletApi.Models
{
    public record TransferUnreceivedRequest(int pageIndex = 0, int pageSize = 50)
    {
        public class Validator : AbstractValidator<TransferUnreceivedRequest>
        {
            public Validator()
            {
                RuleFor(x => x.pageIndex).GreaterThanOrEqualTo(0);
                RuleFor(x => x.pageSize).GreaterThan(0).LessThanOrEqualTo(50);
            }
        }
    }
}
