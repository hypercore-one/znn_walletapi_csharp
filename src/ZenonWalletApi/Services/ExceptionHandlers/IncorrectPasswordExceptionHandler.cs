using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Zenon.Wallet;

namespace ZenonWalletApi.Services.ExceptionHandlers
{
    internal class IncorrectPasswordExceptionHandler : IExceptionHandler
    {
        public IncorrectPasswordExceptionHandler(ILogger<IncorrectPasswordExceptionHandler> logger)
        {
            Logger = logger;
        }

        private ILogger<IncorrectPasswordExceptionHandler> Logger { get; }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not IncorrectPasswordException)
            {
                return false;
            }

            Logger.LogWarning("Incorrent password");

            httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = httpContext.Response.StatusCode,
                Detail = "Incorrent password",
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
