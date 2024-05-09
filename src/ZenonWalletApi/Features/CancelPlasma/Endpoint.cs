﻿using Microsoft.AspNetCore.Mvc;
using Zenon.Model.NoM.Json;
using Zenon.Wallet;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.CancelPlasma
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapCancelPlasmaEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/{account}/cancel", CancelPlasmaAsync)
                .WithName("CancelPlasma")
                .Produces<JAccountBlockTemplate>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Cancel a plasma fusion
        /// </summary>
        /// <remarks>
        /// <para>The QSR used for the plasma fusion is send back</para>
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        /// <param name="wallet"></param>
        /// <param name="client"></param>
        /// <param name="account" example="z1qqjnwjjpnue8xmmpanz6csze6tcmtzzdtfsww7 or 0">The account address or index to cancel from</param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        public static async Task<JAccountBlockTemplate> CancelPlasmaAsync(
            IWalletService wallet,
            INodeService client,
            [Validate] AccountString account,
            [FromBody][Validate] CancelPlasmaRequest request,
            CancellationToken cancellationToken = default)
        {
            await client.ConnectAsync(cancellationToken);

            // Wallet account
            IWalletAccount walletAccount;

            if (account.Address != null)
            {
                // Access wallet account from address
                walletAccount = await wallet.GetAccountAsync(account.Address!);
            }
            else
            {
                // Access wallet account from index
                walletAccount = await wallet.GetAccountAsync(account.Index!.Value);
            }

            // Create block
            var block = client.Api.Embedded.Plasma.Cancel(request.IdHash);

            // Send block
            var response = await client.SendAsync(block, walletAccount, cancellationToken);

            return response.ToJson();
        }
    }
}
