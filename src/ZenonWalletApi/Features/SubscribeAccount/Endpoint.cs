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
                .MapPut("/{accountIndex}", SubscribeAccountAsync)
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
        /// <para>Subscribe a wallet account to auto-receive account blocks</para>
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        public static async Task<IResult> SubscribeAccountAsync(
            IAutoReceiverService autoReceiver,
            [Validate] AccountIndex accountIndex)
        {
            await autoReceiver.SubscribeAsync(accountIndex.value);

            return Results.Ok();
        }
    }
}
