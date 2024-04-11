using Zenon.Model.Embedded.Json;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetPlasmaInfo
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetPlasmaInfoEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("{address}/plasma", async (
                    INodeService client,
                    [Validate] AddressString address
                    ) =>
                    {
                        await client.ConnectAsync();

                        // Retrieve plasma info
                        var result = await client.Api.Embedded.Plasma.Get(address.value);

                        return result.ToJson();
                    })
                .WithName("GetPlasmaInfo")
                .WithDescription("Gets the current plasma, max plasma and fused qsr amount of the supplied address")
                .Produces<JPlasmaInfo>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
