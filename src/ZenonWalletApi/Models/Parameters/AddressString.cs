using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models.Parameters
{
    public record AddressString(Address value) : IParsable<AddressString>
    {
        public static AddressString Parse(string s, IFormatProvider? provider)
        {
            return new AddressString(Address.Parse(s));
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AddressString result)
        {
            try
            {
                result = Parse(s!, provider);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public class Validator : AbstractValidator<AddressString>
        {
            public Validator()
            {
                RuleFor(x => x.value).NotEqual(Address.EmptyAddress);
            }
        }
    }
}
