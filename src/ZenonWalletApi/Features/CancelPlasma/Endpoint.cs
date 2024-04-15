using Microsoft.AspNetCore.Mvc;
using Zenon.Model.NoM.Json;
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
                .MapPost("/{accountIndex}/cancel", CancelPlasmaAsync)
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

        /// <remarks>
        /// Cancels a plasma fusion and receive the QSR back
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        public static async Task<JAccountBlockTemplate> CancelPlasmaAsync(
            IWalletService wallet,
            INodeService client,
            [Validate] AccountIndex accountIndex,
            [FromBody][Validate] CancelPlasmaRequest request)
        {
            await client.ConnectAsync();

            // Access wallet account from index
            var account = await wallet.GetAccountAsync(accountIndex.value);

            // Create block
            var block = client.Api.Embedded.Plasma.Cancel(request.IdHash);

            // Send block
            var response = await client.SendAsync(block, account);

            return response.ToJson();
        }
    }
}
