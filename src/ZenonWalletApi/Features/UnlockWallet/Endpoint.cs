using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.UnlockWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapUnlockWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/unlock", UnlockWalletAsync)
                .WithName("UnlockWallet")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Unlock the wallet
        /// </summary>
        /// <remarks>
        /// <para>Unlocking the wallet will decrypt the encrypted wallet file and load it to memory</para>
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized</para>
        /// </remarks>
        public static async Task<IResult> UnlockWalletAsync(
            IWalletService wallet,
            [Validate] UnlockWalletRequest request,
            CancellationToken cancellationToken = default)
        {
            await wallet.UnlockAsync(request.Password);

            return Results.Ok();
        }
    }
}
