using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using Zenon;
using Zenon.Model.NoM;
using Zenon.Model.NoM.Json;
using Zenon.Model.Primitives;
using Zenon.Utils;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Exceptions;
using ZenonWalletApi.Models.Parameters;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Features.SendTransfer
{
    internal static class Endpoint
    {
        public static IEndpointRouteBuilder MapSendTransferEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapPost("/{address}/send", SendTransferAsync)
                .WithName("SendTransfer")
                .Produces<JAccountBlockTemplate>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
                .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesValidationProblem()
                .RequireAuthorization("User");
            return endpoints;
        }

        /// <summary>
        /// Send tokens to an address
        /// </summary>
        /// <remarks>
        /// <para>Requires User authorization policy</para>
        /// <para>Requires Wallet to be initialized and unlocked</para>
        /// </remarks>
        public static async Task<JAccountBlockTemplate> SendTransferAsync(
            IWalletService wallet,
            INodeService client,
            [Validate] AddressString address,
            [FromBody][Validate] SendTransferRequest request)
        {
            await client.ConnectAsync();

            // Access wallet account
            var account = await wallet.GetAccountAsync(address.value);

            BigInteger amount;
            if (request.TokenStandard == TokenStandard.ZnnZts ||
                request.TokenStandard == TokenStandard.QsrZts)
            {
                amount = AmountUtils.ExtractDecimals(request.Amount, Constants.CoinDecimals);
            }
            else
            {
                var token = await client.Api.Embedded.Token.GetByZts(request.TokenStandard);
                if (token == null)
                {
                    throw new NotFoundException("Token does not exist");
                }
                amount = AmountUtils.ExtractDecimals(request.Amount, (int)token.Decimals);
            }

            // Retrieve account info
            var accountInfo = await client.Api.Ledger
                .GetAccountInfoByAddress(address.value);

            // Find balance info
            var balanceInfo = accountInfo.BalanceInfoList
                .FirstOrDefault(x => x.Token.TokenStandard == request.TokenStandard);

            // Check balance
            if (balanceInfo == null)
            {
                throw new NotFoundException($"You do not have any {request.TokenStandard} tokens");
            }
            else if (balanceInfo.Balance < amount)
            {
                if (balanceInfo.Balance == BigInteger.Zero)
                {
                    throw new NotFoundException($"You do not have any {balanceInfo.Token.Symbol} tokens");
                }
                else
                {
                    throw new NotFoundException($"You do not have enough {balanceInfo.Token.Symbol} tokens");
                }
            }

            // Create send block
            var block = AccountBlockTemplate.Send(
                client.ProtocolVersion, client.ChainIdentifier,
                request.Address, request.TokenStandard, amount);

            // Send block
            var response = await client.SendAsync(block, account);

            // Return block hash
            return response.ToJson();
        }
    }
}
