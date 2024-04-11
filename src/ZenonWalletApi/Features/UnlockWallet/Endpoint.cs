using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.UnlockWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapUnlockWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost(
                    "/unlock", async (
                        IWalletService service,
                        [Validate] UnlockWalletRequest request
                    ) =>
                    {
                        await service.UnlockAsync(request.Password);

                        return Results.Ok();
                    })
                .WithName("UnlockWallet")
                .WithDescription("Unlocks the wallet")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
