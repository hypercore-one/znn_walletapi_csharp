using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.LockWallet
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapLockWalletEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/lock", LockWalletAsync)
                .WithName("LockWallet")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Lock the wallet
        /// </summary>
        /// <remarks>
        /// <para>Locking the wallet unloads it from memory</para>
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        public static async Task<IResult> LockWalletAsync(
            IWalletService service,
            CancellationToken cancellationToken = default)
        {
            await service.LockAsync();

            return Results.Ok();
        }
    }
}
