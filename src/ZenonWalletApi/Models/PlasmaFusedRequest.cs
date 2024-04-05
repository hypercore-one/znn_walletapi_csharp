using FluentValidation;

namespace ZenonWalletApi.Models
{
    public record PlasmaFusedRequest(int pageIndex = 0, int pageSize = 1024)
    {
        public class Validator : AbstractValidator<PlasmaFusedRequest>
        {
            public Validator()
            {
                RuleFor(x => x.pageIndex).GreaterThanOrEqualTo(0);
                RuleFor(x => x.pageSize).GreaterThan(0).LessThanOrEqualTo(1024);
            }
        }
    }
}
