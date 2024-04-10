using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record ReceiveTransferRequest
    {
        [Required]
        public required Hash BlockHash { get; set; }

        public class Validator : AbstractValidator<ReceiveTransferRequest>
        {
            public Validator()
            {
                RuleFor(x => x.BlockHash).NotNull().NotEqual(Hash.Empty);
            }
        }
    }
}
