using Immunitas.Application;

namespace Immunitas.Portal.Bff.Middlewares;

public class UserContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserContext userContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            userContext.Init(context.User);
        }

        await next(context);
    }
}
