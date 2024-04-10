using FluentValidation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Models
{
    public record TransferUnreceivedRequest(int pageIndex = 0, int pageSize = 50)
    {
        [DefaultValue(0)]
        public int pageIndex { get; set; } = pageIndex;

        [DefaultValue(50)]
        [Range(1, 50)]
        public int pageSize { get; set; } = pageSize;

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
