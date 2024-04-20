using FluentValidation;
using System.ComponentModel;

namespace ZenonWalletApi.Models
{
    public record AddWalletAccountsRequest(int numberOfAccounts = 1)
    {
        [DefaultValue(1)]
        public int numberOfAccounts { get; set; } = numberOfAccounts;

        public class Validator : AbstractValidator<AddWalletAccountsRequest>
        {
            public Validator()
            {
                RuleFor(x => x.numberOfAccounts).GreaterThan(0);
            }
        }
    }
}
