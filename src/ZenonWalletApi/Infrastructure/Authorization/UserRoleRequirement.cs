using Microsoft.AspNetCore.Authorization;

namespace ZenonWalletApi.Infrastructure.Authorization
{
    public class UserRoleRequirement : IAuthorizationRequirement
    {
        public UserRoleRequirement(params string[] roles)
        {
            Roles = roles;
        }

        public string[] Roles { get; }
    }
}
