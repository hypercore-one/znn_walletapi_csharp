using Zenon.Model.Embedded.Json;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetFusionInfo
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetFusionInfoEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/{address}/fused", GetFusionInfoAsync)
                .WithName("GetFusionInfo")
                .Produces<JFusionEntryList>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Get all fusion entries by address
        /// </summary>
        /// <remarks>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static async Task<JFusionEntryList> GetFusionInfoAsync(
            IWalletService wallet,
            INodeService client,
            [Validate] AddressString address,
            [AsParameters][Validate] PlasmaFused request)
        {
            await client.ConnectAsync();

            var syncInfo = await client.Api.Stats.SyncInfo();

            // Retrieve plasma info
            var result = await client.Api.Embedded.Plasma
                .GetEntriesByAddress(address.value, (uint)request.pageIndex, (uint)request.pageSize);
            
            var json = result.ToJson();

            foreach (var element in json.list)
            {
                element.isRevocable = syncInfo.currentHeight > element.expirationHeight;
            }

            return json!;
        }
    }
}
