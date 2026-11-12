using System.Threading.RateLimiting;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace AllocatrApi.Configuration;

public static class PasswordResetFlowExtensions
{
    public static IServiceCollection AddAllocatrPasswordResetFlow(this IServiceCollection services)
    {
        services.AddScoped<PasswordResetSender>();
        services.AddTransient<AllocatrPasswordResetTokenProvider>();
        services.Configure<AllocatrPasswordResetTokenOptions>(options =>
        {
            options.Name = "AllocatrPasswordReset";
            options.TokenLifespan = TimeSpan.FromHours(1);
        });
        services.Configure<IdentityOptions>(options =>
        {
            options.Tokens.ProviderMap["AllocatrPasswordReset"] =
                new TokenProviderDescriptor(typeof(AllocatrPasswordResetTokenProvider));
            options.Tokens.PasswordResetTokenProvider = "AllocatrPasswordReset";
        });
        services.AddRateLimiter(options => options.AddPolicy("password-reset-request", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5, Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0, AutoReplenishment = true,
                })));
        return services;
    }
}

public sealed class AllocatrPasswordResetTokenOptions : DataProtectionTokenProviderOptions { }

public sealed class AllocatrPasswordResetTokenProvider : DataProtectorTokenProvider<AllocatrUser>
{
    public AllocatrPasswordResetTokenProvider(IDataProtectionProvider protection,
        IOptions<AllocatrPasswordResetTokenOptions> options,
        ILogger<DataProtectorTokenProvider<AllocatrUser>> logger)
        : base(protection, options, logger) { }
}
