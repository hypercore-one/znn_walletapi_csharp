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
                .MapGet("/{address}/balances", GetAccountInfoAsync)
                .WithName("GetAccountInfo")
                .Produces<JAccountInfo>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Get the account info by address
        /// </summary>
        /// <remarks>
        /// <para>Get the account height and token balances by address</para>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static async Task<JAccountInfo> GetAccountInfoAsync(
            INodeService client,
            [Validate] AddressString address)
        {
            await client.ConnectAsync();

            var result = await client.Api.Ledger.GetAccountInfoByAddress(address.value);

            return result.ToJson();
        }
    }
}
