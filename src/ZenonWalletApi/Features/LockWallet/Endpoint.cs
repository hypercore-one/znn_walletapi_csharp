using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.LockWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapLockWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost(
                    "/lock", async (
                        IWalletService service
                    ) =>
                    {
                        await service.LockAsync();

                        return Results.Ok();
                    })
                .WithName("LockWallet")
                .WithDescription("Locks the wallet")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
