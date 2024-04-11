using Zenon.Model.NoM.Json;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetUnreceivedAccountBlocks
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetUnreceivedAccountBlocksEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/{address}/unreceived", async (
                    IWalletService wallet,
                    INodeService client,
                    [Validate] AddressString address,
                    [AsParameters][Validate] TransferUnreceivedRequest request
                    ) =>
                    {
                        await client.ConnectAsync();

                        // Retrieve all unreceived account blocks by address
                        var result = await client.Api.Ledger
                            .GetUnreceivedBlocksByAddress(address.value,
                                pageIndex: (uint)request.pageIndex,
                                pageSize: (uint)request.pageSize);

                        return result.ToJson();
                    })
                .WithName("GetUnreceivedAccountBlocks")
                .WithDescription("Gets the unreceived account blocks by address")
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
