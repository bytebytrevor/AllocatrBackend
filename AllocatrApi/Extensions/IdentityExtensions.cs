using System;
using AllocatrApi.Data;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity;

namespace AllocatrApi.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityConfig(this IServiceCollection services)
    {
        services.AddIdentity<AllocatrUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<AllocatrDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
