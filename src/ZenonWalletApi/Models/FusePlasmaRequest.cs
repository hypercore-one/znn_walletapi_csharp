using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record FusePlasmaRequest
    {
        [Required]
        public required Address Address { get; set; }

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
