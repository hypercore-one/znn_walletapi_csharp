using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.RestoreWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapRestoreWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost(
                    "/restore", async (
                        IWalletService service,
                        [Validate] RestoreWalletRequest request
                    ) =>
                    {
                        await service.RestoreAsync(request.Password, request.Mnemonic);

                        return Results.Ok();
                    })
                .WithName("RestoreWallet")
                .WithDescription("Restores an existing wallet")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("Admin");
            return endpoints;
        }
    }
}
