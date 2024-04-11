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
                .MapPost("/{accountIndex}/cancel", async (
                    IWalletService wallet,
                    INodeService client,
                    [Validate] AccountIndex accountIndex,
                    [FromBody][Validate] CancelPlasmaRequest request
                    ) =>
                    {
                        await client.ConnectAsync();

                        // Access wallet account from index
                        var account = await wallet.GetAccountAsync(accountIndex.value);

                        // Retrieve wallet account address
                        var address = await account.GetAddressAsync();

                        // Create block
                        var block = client.Api.Embedded.Plasma.Cancel(request.IdHash);

                        // Send block
                        var response = await client.SendAsync(block, account);

                        return response.ToJson();
                    })
                .WithName("CancelPlasma")
                .WithDescription("Cancels a plasma fusion and receive the QSR back")
                .Produces<JAccountBlockTemplate>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
