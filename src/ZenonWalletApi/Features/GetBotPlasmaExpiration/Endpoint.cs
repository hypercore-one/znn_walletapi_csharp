using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetBotPlasmaExpiration
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetBotPlasmaExpirationEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/plasma-bot/expiration/{address}", GetBotPlasmaExpirationAsync)
                .WithName("GetBotPlasmaExpiration")
                .Produces<DateTime?>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Get the fusion expiration by address from the plasma-bot
        /// </summary>
        /// <remarks>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static async Task<GetBotPlasmaExpirationResponse> GetBotPlasmaExpirationAsync(
            IPlasmaBotService plasmaBot,
            [Validate] AddressString address,
            CancellationToken cancellationToken = default)
        {
            var expiry = await plasmaBot.GetExpirationAsync(address.value, cancellationToken);

            return new GetBotPlasmaExpirationResponse(expiry);
        }
    }
}
