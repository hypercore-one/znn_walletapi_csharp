using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ZenonWalletApi.Infrastructure.Authorization
{
    public class UserRoleRequirementAuthorizationHandler : AuthorizationHandler<UserRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRoleRequirement requirement)
        {
            if (!context.User.HasClaim(x => x.Type == ClaimTypes.Role))
                return Task.CompletedTask;

            var userRoles = context.User.Claims.Where(x => x.Type == ClaimTypes.Role);

            var hasRoles = requirement.Roles.All(x => userRoles.Any(y => y.Value == x));

            if (hasRoles)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
