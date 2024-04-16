using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record FuseBotPlasmaRequest
    {
        /// <summary>
        /// The beneficiary address
        /// </summary>
        /// <example>z1qqjnwjjpnue8xmmpanz6csze6tcmtzzdtfsww7</example>
        [Required]
        public required Address Address { get; set; }

        public class Validator : AbstractValidator<FuseBotPlasmaRequest>
        {
            public Validator()
            {
                RuleFor(x => x.Address).NotNull().NotEqual(Address.EmptyAddress);
            }
        }
    }
}
