using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ZenonWalletApi.Models;

namespace ZenonWalletApi.Services
{
    public interface IUserService
    {
        Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest model);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
    }

    public class ApiOptions
    {
        public const string Api = "Api";

        public required IEnumerable<User> Users { get; set; }
    }

    public class UserService : IUserService
    {
        public UserService(ILogger<UserService> logger, IOptions<ApiOptions> appSettings, IJwtService jwt)
        {
            Logger = logger;
            Options = appSettings.Value;
            Jwt = jwt;
        }

        private ILogger<UserService> Logger { get; }

        private ApiOptions Options { get; }

        private IJwtService Jwt { get; }

        public async Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest request)
        {
            Logger.LogInformation($"Authenticate: {request.Username}");

            var user = Options.Users.FirstOrDefault(x => x.IsActive &&
                string.Equals(x.Username, request.Username, StringComparison.OrdinalIgnoreCase));

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var authClaims = new List<Claim>
            {
               new Claim(ClaimTypes.Name, user.Username),
               new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
            };

            foreach (var userRole in user.Roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = await Jwt.GenerateJwtTokenAsync(authClaims);

            return new AuthenticateResponse(user, token);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await Task.Run(() => Options.Users);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await Task.Run(() => Options.Users.FirstOrDefault(x => x.Id == id));
        }
    }
}
