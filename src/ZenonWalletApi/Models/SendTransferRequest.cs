using FluentValidation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record SendTransferRequest
    {
        /// <summary>
        /// The beneficiary address
        /// </summary>
        [Required]
        public required Address Address { get; set; }

        /// <summary>
        /// The amount to send
        /// </summary>
        /// <example>10.05</example>
        [Required]
        public required string Amount { get; set; }

        /// <summary>
        /// The token to send
        /// </summary>
        /// <example>QSR</example>
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
