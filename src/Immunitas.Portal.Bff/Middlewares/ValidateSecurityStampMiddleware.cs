using Immunitas.Domain.Entities.Users;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Immunitas.Portal.Bff.Middlewares;

public class ValidateSecurityStampMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IRepository<User> usersRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && !context.Request.Path.ToString().Contains("Login"))
        {
            var tokenStamp = context.User.Claims.FirstOrDefault(c => c.Type == "stamp")?.Value;
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (tokenStamp != null && int.TryParse(userId, out var userIdInt))
            {
                var user = await usersRepository
                    .GetById(userIdInt)
                    .Select(u => new { u.SecurityStamp })
                    .FirstOrDefaultAsync();
                if (user == null || user.SecurityStamp != tokenStamp)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;                    
                    return;
                }
            }
        }
        await next(context);
    }
}
