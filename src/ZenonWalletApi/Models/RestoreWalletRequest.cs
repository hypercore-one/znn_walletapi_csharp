using FluentValidation;
using Zenon.Wallet.BIP39;
using ZenonWalletApi.Models.Validators;

namespace ZenonWalletApi.Models
{
    public record RestoreWalletRequest(
        string password, string mnemonic)
    {
        public class Validator : AbstractValidator<RestoreWalletRequest>
        {
            public Validator()
            {
                RuleFor(x => x.password)
                    .WalletPassword();
                RuleFor(x => x.mnemonic)
                    .NotEmpty()
                    .Must(value => new Zenon.Wallet.BIP39.BIP39().ValidateMnemonic(value, BIP39Wordlist.English));
            }
        }
    }
}
