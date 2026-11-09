using AllocatrApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AllocatrApi.Middleware;

// Only these explicitly marked endpoints may be used by an unverified session.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowUnverifiedEmailAttribute : Attribute { }

public sealed class RequireVerifiedEmailMiddleware
{
    private readonly RequestDelegate _next;
    public RequireVerifiedEmailMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, UserManager<AllocatrUser> users)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null || context.User.Identity?.IsAuthenticated != true ||
            endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null ||
            endpoint.Metadata.GetMetadata<AllowUnverifiedEmailAttribute>() != null)
        {
            await _next(context);
            return;
        }

        // Read current state from Identity rather than trusting a stale cookie claim.
        var user = await users.GetUserAsync(context.User);
        if (user == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        if (!user.EmailConfirmed)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                code = "email_verification_required",
                message = "Verify your email address to continue.",
            });
            return;
        }
        await _next(context);
    }
}
