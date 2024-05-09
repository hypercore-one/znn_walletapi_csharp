﻿using Zenon.Model.Embedded.Json;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.GetPlasmaInfo
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapGetPlasmaInfoEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapGet("{address}/plasma", GetPlasmaInfoAsync)
                .WithName("GetPlasmaInfo")
                .Produces<JPlasmaInfo>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Get plasma info by address
        /// </summary>
        /// <remarks>
        /// <para>Get the current plasma, max plasma and fused QSR amount of the supplied address</para>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static async Task<JPlasmaInfo> GetPlasmaInfoAsync(
            INodeService client,
            [Validate] AddressString address,
            CancellationToken cancellationToken = default)
        {
            await client.ConnectAsync(cancellationToken);

            // Retrieve plasma info
            var result = await client.Api.Embedded.Plasma.Get(address.value);

            return result.ToJson();
        }
    }
}
