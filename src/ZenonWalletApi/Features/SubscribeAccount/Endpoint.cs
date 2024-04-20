using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.SubscribeAccount
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapSubscribeAccountEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPut("/{address}", SubscribeAccountAsync)
                .WithName("SubscribeAccount")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Subscribe a wallet account to auto-receive
        /// </summary>
        /// <remarks>
        /// <para>Subscribe a wallet account to auto-receive account blocks. Unreceived account blocks will only be received when the auto-receiver is enabled and the wallet unlocked.</para>
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        public static async Task<IResult> SubscribeAccountAsync(
            IAutoReceiverService autoReceiver,
            [Validate] AddressString address)
        {
            await autoReceiver.SubscribeAsync(address.value);

            return Results.Ok();
        }
    }
}
