using Immunitas.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Immunitas.Portal.Bff.ExceptionHandlers;

public class EntityNotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is EntityNotFoundException entityNotFoundException)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Объект не найден",
                Detail = $"Объект '{entityNotFoundException.EntityName}' с ключом '{entityNotFoundException.Key}' не найден.",
                Status = StatusCodes.Status404NotFound
            };

            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
        return false;
    }
}
