using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetWalletStatus
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetWalletStatusEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet(
                    "/status", (
                        IWalletService service
                    ) =>
                    {
                        return new WalletStatusResponse(service.IsInitialized, service.IsUnlocked);
                    })
                .WithName("GetWalletStatus")
                .WithDescription("Gets the wallet status indicating whether the wallet is initialized and unlocked")
                .Produces<WalletStatusResponse>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
