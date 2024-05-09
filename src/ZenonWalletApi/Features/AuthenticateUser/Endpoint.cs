using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.AuthenticateUser
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapAuthenticateUserEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/authenticate", AuthenticateUserAsync)
                .WithName("AuthenticateUser")
                .Produces<AuthenticateResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .ProducesValidationProblem()
                .AllowAnonymous();
            return endpoints;
        }

        /// <summary>
        /// Authenticate an user
        /// </summary>
        public static async Task<IResult> AuthenticateUserAsync(
            IUserService users,
            [Validate] AuthenticateRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await users.AuthenticateAsync(request);

            if (result == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(result!);
        }
    }
}
