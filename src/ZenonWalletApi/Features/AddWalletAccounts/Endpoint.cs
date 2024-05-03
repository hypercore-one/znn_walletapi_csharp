using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.AddWalletAccounts
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapAddWalletAccountsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/accounts", AddWalletAccountAddressAsync)
                .WithName("AddWalletAccounts")
                .Produces<WalletAccountList>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Add wallet accounts
        /// </summary>
        /// <remarks>
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        public static async Task<WalletAccountList> AddWalletAccountAddressAsync(
            IWalletService wallet,
            [AsParameters][Validate] AddWalletAccountsRequest request)
        {
            return await wallet.AddAccountsAsync(request.numberOfAccounts);
        }
    }
}
