using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AllocatrApi.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
                )

            };

            // For HttpOnly cookies
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    ctx.Token = ctx.Request.Cookies["access_token"];
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
