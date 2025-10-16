using Compliance_Tracker.Models;
using Microsoft.AspNetCore.Identity;

namespace Compliance_Tracker.Middleware;

public class SetupCheckMiddleware
{
    private readonly RequestDelegate _next;

    public SetupCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        // Skip if already going to setup or accessing static files
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        if (path.StartsWith("/setup") ||
            path.StartsWith("/css") ||
            path.StartsWith("/js") ||
            path.StartsWith("/lib") ||
            path.StartsWith("/favicon"))
        {
            await _next(context);
            return;
        }

        // Check if any users exist
        var usersExist = userManager.Users.Any();

        if (!usersExist && !path.StartsWith("/setup"))
        {
            context.Response.Redirect("/Setup");
            return;
        }

        await _next(context);
    }
}

public static class SetupCheckMiddlewareExtensions
{
    public static IApplicationBuilder UseSetupCheck(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SetupCheckMiddleware>();
    }
}
