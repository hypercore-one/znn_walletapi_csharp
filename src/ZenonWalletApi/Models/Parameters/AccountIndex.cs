using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace ZenonWalletApi.Models.Parameters
{
    public record AccountIndex(int value = 0) : IParsable<AccountIndex>
    {
        public static AccountIndex Parse(string s, IFormatProvider? provider)
        {
            return new AccountIndex(int.Parse(s, provider));
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AccountIndex result)
        {
            if (int.TryParse(s, out var parsed))
            {
                result = new AccountIndex(parsed);
                return true;
            }

            result = default;
            return false;
        }

        public class Validator : AbstractValidator<AccountIndex>
        {
            public Validator()
            {
                RuleFor(x => x.value).GreaterThanOrEqualTo(0);
            }
        }
    }
}
