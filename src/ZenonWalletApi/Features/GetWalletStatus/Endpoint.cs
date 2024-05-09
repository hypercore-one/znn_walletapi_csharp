using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetWalletStatus
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetWalletStatusEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/status", GetWalletStatus)
                .WithName("GetWalletStatus")
                .Produces<WalletStatusResponse>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Get the wallet status
        /// </summary>
        /// <remarks>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static WalletStatusResponse GetWalletStatus(
            IWalletService wallet,
            CancellationToken cancellationToken = default)
        {
            return new WalletStatusResponse(wallet.IsInitialized, wallet.IsUnlocked);
        }
    }
}
