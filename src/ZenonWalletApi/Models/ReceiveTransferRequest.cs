using FluentValidation;
using System.ComponentModel.DataAnnotations;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models
{
    public record ReceiveTransferRequest
    {
        /// <summary>
        /// The hash of the account block to receive
        /// </summary>
        /// <example>126e53dd8a5514d67e0203e580c0d9950c9b0255618d53e87e42bdbd18246e9f</example>
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
