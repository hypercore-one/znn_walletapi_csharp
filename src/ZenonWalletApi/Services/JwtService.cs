using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ZenonWalletApi.Services
{
    public interface IJwtService
    {
        Task<string> GenerateJwtTokenAsync(IEnumerable<Claim> claims);
    }

    public class JwtOptions
    {
        public const string Jwt = "Api:Jwt";

        [Required]
        public required string Secret { get; set; }
        [Required]
        public required string ValidIssuer { get; set; } = "zenon.wallet.api";
        [Required]
        public required string ValidAudience { get; set; } = "zenon.network";
        [AllowNull]
        public DateTime? ExpiresOn { get; set; }
        [AllowNull]
        public TimeSpan? ExpiresAfter { get; set; }
    }

    public class JwtService : IJwtService
    {
        public JwtService(IOptions<JwtOptions> options)
        {
            Options = options.Value;
        }

        private JwtOptions Options { get; set; }

        public async Task<string> GenerateJwtTokenAsync(IEnumerable<Claim> claims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.Secret));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = Options.ValidIssuer,
                Audience = Options.ValidAudience,
                Expires = Options.ExpiresAfter.HasValue
                    ? DateTime.UtcNow.Add(Options.ExpiresAfter.Value)
                    : Options.ExpiresOn,
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            };

            return await Task.Run(() =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            });
        }
    }
}
