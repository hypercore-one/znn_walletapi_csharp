using Microsoft.AspNetCore.Mvc;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.FuseBotPlasma
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapFuseBotPlasmaEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/plasma-bot/fuse", FuseBotPlasmaAsync)
                .WithName("FuseBotPlasma")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <remarks>
        /// Generates plasma by fusing QSR to an address from the community plasma-bot
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static async Task<IResult> FuseBotPlasmaAsync(
            IPlasmaBotService plasmaBot,
            [FromBody][Validate] FuseBotPlasmaRequest request)
        {
            return await plasmaBot.FuseAsync(request.Address);
        }
    }
}
