using FluentValidation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models.Parameters
{
    public record TransferUnreceived(int pageIndex = 0, int pageSize = 50)
    {
        [DefaultValue(0)]
        public int pageIndex { get; } = pageIndex;

        [Range(1, 50)]
        [DefaultValue(50)]
        public int pageSize { get; } = pageSize;

        public class Validator : AbstractValidator<TransferUnreceived>
        {
            public Validator()
            {
                RuleFor(x => x.pageIndex).GreaterThanOrEqualTo(0);
                RuleFor(x => x.pageSize).GreaterThan(0).LessThanOrEqualTo(50);
            }
        }
    }
}
