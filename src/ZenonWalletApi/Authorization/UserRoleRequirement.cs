using Microsoft.AspNetCore.Authorization;

namespace ZenonWalletApi.Authorization
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
