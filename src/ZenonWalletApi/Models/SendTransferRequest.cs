using FluentValidation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record SendTransferRequest
    {
        [Required]
        public required Address Address { get; set; }

        [Required]
        public required string Amount { get; set; }

        [DefaultValue("ZNN")]
        public TokenStandard? TokenStandard { get; set; } = TokenStandard.ZnnZts;

        public class Validator : AbstractValidator<SendTransferRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Address).NotNull().NotEqual(Address.EmptyAddress);
                RuleFor(x => x.Amount).WalletAmount();
                RuleFor(x => x.TokenStandard).NotNull().NotEqual(TokenStandard.EmptyZts);
            }
        }
    }
}
