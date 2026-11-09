using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;

namespace AllocatrApi.Configuration;

public static class EmailVerificationFlowExtensions
{
    public static IServiceCollection AddAllocatrEmailVerificationFlow(this IServiceCollection services)
    {
        services.AddScoped<EmailVerificationSender>();
        // This design uses an authenticated, unverified session for the inbox
        // screen. The middleware, rather than the sign-in option, gates API access.
        services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
        });
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("verification-email", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1,
                        Window = TimeSpan.FromSeconds(60),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));
            options.AddPolicy("auth-attempts", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)) : 60;
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers["Retry-After"] = seconds.ToString(CultureInfo.InvariantCulture);
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    message = "Please wait a moment before trying again.",
                    retryAfterSeconds = seconds,
                }, cancellationToken);
            };
        });
        return services;
    }
}
