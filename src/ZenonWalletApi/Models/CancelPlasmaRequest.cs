using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record CancelPlasmaRequest
    {
        [Required]
        public required Hash IdHash { get; set; }

        public class Validator : AbstractValidator<CancelPlasmaRequest>
        {
            public Validator()
            {
                RuleFor(x => x.IdHash).NotNull().NotEqual(Hash.Empty);
            }
        }
    }
}
