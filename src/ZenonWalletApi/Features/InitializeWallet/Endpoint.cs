using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.InitializeWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapInitializeWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/init", InitializeWalletAsync)
                .WithName("InitializeWallet")
                .Produces<InitWalletResponse>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("Admin");
            return endpoints;
        }

        /// <summary>
        /// Initialize a new wallet
        /// </summary>
        /// <remarks>
        /// <para>Initializing a wallet creates a new encrypted wallet file with a random seed</para>
        /// <para>Requires Admin authorization policy</para>
        /// </remarks>
        public static async Task<InitWalletResponse> InitializeWalletAsync(
            IWalletService service,
            [Validate] InitWalletRequest request,
            CancellationToken cancellationToken = default)
        {
            var mnemonic = await service.InitAsync(request.Password);

            return new InitWalletResponse(mnemonic);
        }
    }
}
