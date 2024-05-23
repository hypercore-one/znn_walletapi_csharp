using FluentValidation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Zenon;

namespace ZenonWalletApi.Models.Parameters
{
    public record GetWalletAccounts(int pageIndex = 0, int pageSize = Constants.RpcMaxPageSize)
    {
        [DefaultValue(0)]
        public int pageIndex { get; } = pageIndex;

        [Range(1, Constants.RpcMaxPageSize)]
        [DefaultValue(Constants.RpcMaxPageSize)]
        public int pageSize { get; } = pageSize;

        public class Validator : AbstractValidator<GetWalletAccounts>
        {
            public Validator()
            {
                RuleFor(x => x.pageIndex).GreaterThanOrEqualTo(0);
                RuleFor(x => x.pageSize).GreaterThan(0).LessThanOrEqualTo(Constants.RpcMaxPageSize);
            }
        }
    }
}
