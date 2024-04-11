using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;
using System.Net;

namespace ZenonWalletApi.Infrastructure.ExceptionHandlers
{
    internal class RemoteInvocationExceptionHandler : IExceptionHandler
    {
        public RemoteInvocationExceptionHandler(ILogger<RemoteInvocationExceptionHandler> logger)
        {
            Logger = logger;
        }

        private ILogger<RemoteInvocationExceptionHandler> Logger { get; }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not RemoteInvocationException)
            {
                return false;
            }

            Logger.LogError(exception, "Remote invocation exception");

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
