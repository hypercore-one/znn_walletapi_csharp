using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record CancelPlasmaRequest
    {
        /// <summary>
        /// The hash of the fusion id
        /// </summary>
        /// <example>d12e37ff24d16ce146e2dd6e30b7a1f1b72ae2cfd00365c1512ca9f4463a51e9</example>
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
