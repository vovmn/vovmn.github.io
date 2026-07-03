using Immunitas.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.ExceptionHandlers;

public class UserUnauthorizedExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is UserUnauthorizedException)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Пользователь не авторизован",
                Detail = "Пользователь не авторизован. Пожалуйста, войдите в систему и повторите попытку.",
                Status = StatusCodes.Status401Unauthorized
            };
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
        return false;
    }
}
