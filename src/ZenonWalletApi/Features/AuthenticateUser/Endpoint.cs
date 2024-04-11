using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.AuthenticateUser
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapAuthenticateUserEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost(
                    "/authenticate",
                    async (
                        IUserService users,
                        [Validate] AuthenticateRequest request
                    ) =>
                    {
                        var result = await users.AuthenticateAsync(request);

                        if (result == null)
                        {
                            return Results.Unauthorized();
                        }

                        return Results.Ok(result!);
                    })
                .WithName("AuthenticateUser")
                .WithDescription("Authenticates an user")
                .Produces<AuthenticateResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .ProducesValidationProblem()
                .AllowAnonymous();
            return endpoints;
        }
    }
}
