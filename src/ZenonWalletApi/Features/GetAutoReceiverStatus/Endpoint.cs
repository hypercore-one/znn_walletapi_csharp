using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetAutoReceiverStatus
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetAutoReceiverStatusEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/status", (
                    IAutoReceiverService service
                    ) =>
                    {
                        return new AutoReceiverStatusResponse(service.IsEnabled);
                    })
                .WithName("GetAutoReceiverStatus")
                .WithDescription("Gets the auto-receiver status indicating whether the service is enabled")
                .Produces<AutoReceiverStatusResponse>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .RequireAuthorization("User");
            return endpoints;
        }
    }
}
