using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Zenon.Wallet;

namespace ZenonWalletApi.Services.ExceptionHandlers
{
    internal class WalletExceptionHandler : IExceptionHandler
    {
        public WalletExceptionHandler(ILogger<WalletExceptionHandler> logger)
        {
            Logger = logger;
        }

        private ILogger<WalletExceptionHandler> Logger { get; }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not WalletException)
            {
                return false;
            }

            Logger.LogWarning(exception.Message);

            httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = httpContext.Response.StatusCode,
                Detail = exception.Message,
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
