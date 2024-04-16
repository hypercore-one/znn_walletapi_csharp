using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetAutoReceiverStatus
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetAutoReceiverStatusEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("/status", GetAutoReceiverStatus)
                .WithName("GetAutoReceiverStatus")
                .Produces<AutoReceiverStatusResponse>()
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Get the auto-receiver status
        /// </summary>
        /// <remarks>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static AutoReceiverStatusResponse GetAutoReceiverStatus(
            IAutoReceiverService service)
        {
            return new AutoReceiverStatusResponse(service.IsEnabled);
        }
    }
}
