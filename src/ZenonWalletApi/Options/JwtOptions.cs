using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ZenonWalletApi.Options
{
    public class JwtOptions
    {
        public const string Jwt = "Api:Jwt";

        public const string DefaultValidIssuer = "zenon.wallet.api";
        public const string DefaultValidAudience = "zenon.network";

        [Required]
        public required string Secret { get; set; }
        [Required]
        public required string ValidIssuer { get; set; } = DefaultValidIssuer;
        [Required]
        public required string ValidAudience { get; set; } = DefaultValidAudience;
        [AllowNull]
        public DateTime? ExpiresOn { get; set; }
        [AllowNull]
        public TimeSpan? ExpiresAfter { get; set; }
    }
}
