using Zenon.Model.Primitives;
using ZenonWalletApi.Models;

namespace ZenonWalletApi.Features.ValidateAddress
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapValidateAddressEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/address/validate", ValidateAddress)
                .WithName("ValidateAddress")
                .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <remarks>
        /// Validates whether specified address is valid
        /// <para>Requires User authorization policy</para>
        /// </remarks>
        /// <param name="address" example="z1qqjnwjjpnue8xmmpanz6csze6tcmtzzdtfsww7">The address to validate</param>
        public static ValidateAddressResponse ValidateAddress(
            string address)
        {
            try
            {
                var addr = Address.Parse(address);
                return new ValidateAddressResponse(addr != Address.EmptyAddress, addr.IsEmbedded);
            }
            catch
            {
                return new ValidateAddressResponse(false, false);
            }
        }
    }
}
