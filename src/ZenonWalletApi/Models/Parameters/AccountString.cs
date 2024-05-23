using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models.Parameters
{
    /// <summary>
    /// The account address or index
    /// </summary>
    public class AccountString : IParsable<AccountString>
    {
        public AccountString(Address address)
        {
            Address = address;
            Index = null;
        }

        public AccountString(int index)
        {
            Address = null;
            Index = index;
        }

        public static AccountString Parse(string s, IFormatProvider? provider)
        {
            if (Address.IsValid(s))
            {
                return new AccountString(Address.Parse(s));
            }
            else
            {
                return new AccountString(int.Parse(s));
            }
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AccountString result)
        {
            try
            {
                result = Parse(s!, provider);
                return true;
            }
            catch
            {
                result = new AccountString(Address.EmptyAddress);
                return false;
            }
        }

        /// <summary>
        /// The account address
        /// </summary>
        /// <example>z1qqjnwjjpnue8xmmpanz6csze6tcmtzzdtfsww7</example>
        public Address? Address { get; }

        /// <summary>
        /// The account index
        /// </summary>
        /// <example>0</example>
        public int? Index { get; }

        public class Validator : AbstractValidator<AccountString>
        {
            public Validator()
            {
                RuleFor(x => x.Index).GreaterThanOrEqualTo(0).When(x => x.Address == null);
                RuleFor(x => x.Address).NotEqual(Address.EmptyAddress).When(x => x.Index == null);
            }
        }
    }
}
