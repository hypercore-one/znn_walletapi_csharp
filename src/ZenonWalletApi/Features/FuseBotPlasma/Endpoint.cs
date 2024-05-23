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

        /// <summary>
        /// Generate plasma by fusing QSR from the plasma-bot
        /// </summary>
        /// <remarks>
        /// <para>Generate plasma for a limited amount of time to an address by fusing QSR from the community plasma-bot</para>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static async Task<IResult> FuseBotPlasmaAsync(
            IPlasmaBotService plasmaBot,
            [FromBody][Validate] FuseBotPlasmaRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await plasmaBot.FuseAsync(request.Address, cancellationToken);

                return Results.Ok();
            }
            catch (HttpRequestException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: (int)ex.StatusCode!);
            }
        }
    }
}
