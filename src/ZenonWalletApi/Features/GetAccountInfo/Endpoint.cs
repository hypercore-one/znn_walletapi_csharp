using Zenon.Model.NoM.Json;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetAccountInfo
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetAccountInfoEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/{address}/balances", async (
                    INodeService client,
                    [Validate] AddressString address
                    ) =>
                    {
                        await client.ConnectAsync();

                        var result = await client.Api.Ledger.GetAccountInfoByAddress(address.value);

                        return result.ToJson();
                    })
                .WithName("GetAccountInfo")
                .WithDescription("Gets the account height and token balances by address")
                .Produces<JAccountInfo>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
