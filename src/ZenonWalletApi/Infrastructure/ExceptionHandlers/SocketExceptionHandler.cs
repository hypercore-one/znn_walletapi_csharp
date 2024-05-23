using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Zenon.Client;

namespace ZenonWalletApi.Infrastructure.ExceptionHandlers
{
    internal class SocketExceptionHandler : IExceptionHandler
    {
        public SocketExceptionHandler(ILogger<SocketExceptionHandler> logger)
        {
            Logger = logger;
        }

        private ILogger<SocketExceptionHandler> Logger { get; }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not SocketException ||
                exception is not WebSocketException ||
                exception is not NoConnectionException)
            {
                return false;
            }

            Logger.LogError(exception, "Connection exception");

            httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = httpContext.Response.StatusCode,
                Detail = exception.Message,
            }, cancellationToken: cancellationToken);

            return true;
        }
    }
}
