using Zenon.Wallet;
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
                .MapDelete("/{account}", UnsubscribeAccountAsync)
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
        /// <param name="autoReceiver"></param>
        /// <param name="account" example="z1qqjnwjjpnue8xmmpanz6csze6tcmtzzdtfsww7 or 0">The account address or index to unsubscribe</param>
        public static async Task<IResult> UnsubscribeAccountAsync(
            IAutoReceiverService autoReceiver,
            [Validate] AccountString account)
        {
            if (account.Address != null)
            {
                // Access wallet account from address
                await autoReceiver.UnsubscribeAsync(account.Address!);
            }
            else
            {
                // Access wallet account from index
                await autoReceiver.UnsubscribeAsync(account.Index!.Value);
            }

            return Results.Ok();
        }
    }
}
