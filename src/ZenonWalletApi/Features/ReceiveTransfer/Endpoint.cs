using Microsoft.AspNetCore.Mvc;
using Zenon.Model.NoM;
using Zenon.Model.NoM.Json;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.ReceiveTransfer
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapReceiveTransferEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/{accountIndex}/receive", ReceiveTransferAsync)
                .WithName("ReceiveTransfer")
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
        /// Receive an account block by block hash
        /// </summary>
        /// <remarks>
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        public static async Task<JAccountBlockTemplate> ReceiveTransferAsync(
            IWalletService wallet,
            INodeService client,
            [Validate] AccountIndex accountIndex,
            [FromBody][Validate] ReceiveTransferRequest request)
        {
            await client.ConnectAsync();

            // Access wallet account from index
            var account = await wallet.GetAccountAsync(accountIndex.value);

            // Create receive block
            var block = AccountBlockTemplate.Receive(
                client.ProtocolVersion, client.ChainIdentifier,
                request.BlockHash);

            // Send block
            var response = await client.SendAsync(block, account);

            return response.ToJson();
        }
    }
}
