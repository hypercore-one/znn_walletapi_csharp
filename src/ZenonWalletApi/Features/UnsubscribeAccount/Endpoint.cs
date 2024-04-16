using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.UnsubscribeAccount
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapUnsubscribeAccountEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapDelete("/{accountIndex}", UnsubscribeAccountAsync)
                .WithName("UnsubscribeAccount")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Unsubscribe a wallet account from auto-receiving
        /// </summary>
        /// <remarks>
        /// <para>Unsubscribe a wallet account from auto-receiving account blocks</para>
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        public static async Task<IResult> UnsubscribeAccountAsync(
            IAutoReceiverService autoReceiver,
            [Validate] AccountIndex accountIndex)
        {
            await autoReceiver.UnsubscribeAsync(accountIndex.value);

            return Results.Ok();
        }
    }
}
