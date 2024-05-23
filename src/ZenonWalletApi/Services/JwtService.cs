using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZenonWalletApi.Options;

namespace ZenonWalletApi.Services
{
    public interface IJwtService
    {
        Task<string> GenerateJwtTokenAsync(IEnumerable<Claim> claims);
    }

    internal class JwtService : IJwtService
    {
        public JwtService(IOptions<JwtOptions> options)
        {
            Options = options.Value;
        }

        private JwtOptions Options { get; }

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
