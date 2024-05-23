using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.RestoreWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapRestoreWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/restore", RestoreWalletAsync)
                .WithName("RestoreWallet")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("Admin");
            return endpoints;
        }

        /// <summary>
        /// Restore an existing wallet
        /// </summary>
        /// <remarks>
        /// <para>Restoring a wallet will unload and overwrite the previously initialized wallet</para>
        /// <para>Requires Admin authorization policy</para>
        /// </remarks>
        public static async Task<IResult> RestoreWalletAsync(
            IWalletService service,
            [Validate] RestoreWalletRequest request,
            CancellationToken cancellationToken = default)
        {
            await service.RestoreAsync(request.Password, request.Mnemonic);

            return Results.Ok();
        }
    }
}
