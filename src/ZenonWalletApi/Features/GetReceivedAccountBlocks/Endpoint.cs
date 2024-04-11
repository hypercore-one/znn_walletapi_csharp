using Zenon.Model.NoM.Json;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetReceivedAccountBlocks
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetReceivedAccountBlocksEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/{address}/received", async (
                    IWalletService wallet,
                    INodeService client,
                    [Validate] AddressString address,
                    [AsParameters][Validate] TransferReceivedRequest request
                    ) =>
                    {
                        await client.ConnectAsync();

                        // Retrieve all received account blocks by address
                        var result = await client.Api.Ledger
                            .GetAccountBlocksByPage(address.value,
                                pageIndex: (uint)request.pageIndex,
                                pageSize: (uint)request.pageSize);

                        return result.ToJson();
                    })
                .WithName("GetReceivedAccountBlocks")
                .WithDescription("Gets the received account blocks by address")
                .Produces<JAccountBlockList>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
