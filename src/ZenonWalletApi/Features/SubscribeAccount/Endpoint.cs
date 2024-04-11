using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.SubscribeAccount
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapSubscribeAccountEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPut("/{accountIndex}", async (
                    IAutoReceiverService autoReceiver,
                    [Validate] AccountIndex accountIndex
                ) =>
                {
                    await autoReceiver.SubscribeAsync(accountIndex.value);

                    return Results.Ok();
                })
                .WithName("SubscribeAccount")
                .WithDescription("Subscribes an account to auto receive account blocks")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
