using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetWalletAccountAddress
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetWalletAccountAddressEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/{accountIndex}/address", GetWalletAccountAddressAsync)
                .WithName("GetWalletAccountAddress")
                .Produces<WalletAccountAddressResponse>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <remarks>
        /// Gets the wallet account address by account index
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        public static async Task<WalletAccountAddressResponse> GetWalletAccountAddressAsync(
            IWalletService wallet,
            [Validate] AccountIndex accountIndex)
        {
            var address = await wallet.GetAccountAddressAsync(accountIndex.value);

            return new WalletAccountAddressResponse(address);
        }
    }
}
