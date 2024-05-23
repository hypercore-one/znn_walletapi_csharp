using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ZenonWalletApi.Models.Exceptions;

namespace ZenonWalletApi.Infrastructure.ExceptionHandlers
{
    internal class NotFoundExceptionHandler : IExceptionHandler
    {
        public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
        {
            Logger = logger;
        }

        private ILogger<NotFoundExceptionHandler> Logger { get; }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not NotFoundException)
            {
                return false;
            }

            Logger.LogWarning(exception.Message);

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
