using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.InitializeWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapInitializeWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost(
                    "/init", async (
                        IWalletService service,
                        [Validate] InitWalletRequest request
                    ) =>
                    {
                        var mnemonic = await service.InitAsync(request.Password);

                        return new InitWalletResponse(mnemonic);
                    })
                .WithName("InitializeWallet")
                .WithDescription("Initializes a new wallet")
                .Produces<InitWalletResponse>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("Admin");
            return endpoints;
        }
    }
}
