﻿using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.MapGetWalletAccountsEndpoint
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetWalletAccountsEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/accounts", GetWalletAccountsAsync)
                .WithName("GetWalletAccounts")
                .Produces<WalletAccountAddressList>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Get all wallet accounts
        /// </summary>
        /// <remarks>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static async Task<WalletAccountAddressList> GetWalletAccountsAsync(
            IWalletService wallet,
            [AsParameters][Validate] GetWalletAccountsRequest request)
        {
            return await wallet.GetAccountsAsync(request.pageIndex, request.pageSize);
        }
    }
}
