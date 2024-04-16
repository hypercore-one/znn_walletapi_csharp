using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record FusePlasmaRequest
    {
        /// <summary>
        /// The beneficiary address
        /// </summary>
        /// <example>z1qqjnwjjpnue8xmmpanz6csze6tcmtzzdtfsww7</example>
        [Required]
        public required Address Address { get; set; }

        /// <summary>
        /// The amount of QSR to fuse
        /// </summary>
        /// <example>100</example>
        [Required]
        public required string Amount { get; set; }

        public class Validator : AbstractValidator<FusePlasmaRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Address).NotNull().NotEqual(Address.EmptyAddress);
                RuleFor(x => x.Amount).WalletAmount();
            }
        }
    }
}
