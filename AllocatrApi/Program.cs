// using AllocatrApi.Data;
// using AllocatrApi.Models;
// using AllocatrApi.Services;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using System.Text.Json.Serialization;
// using AllocatrApi.Configuration;
// using AllocatrApi.Infrastructure;
// using AllocatrApi.Data.Seed;
// using AllocatrApi.Services.Storage;

// var builder = WebApplication.CreateBuilder(args);

// // ----------------- CORS -----------------
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend", policy =>
//     {
//         policy.WithOrigins("http://localhost:5173")
//               .AllowAnyMethod()
//               .AllowAnyHeader()
//               .AllowCredentials();
//     });
// });

// // ----------------- DB Context -----------------
// builder.Services.AddDbContext<AllocatrDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("Migrations"))
// );

// // ----------------- Identity -----------------
// builder.Services.AddIdentity<AllocatrUser, IdentityRole<Guid>>(options =>
// {
//     options.Password.RequireDigit = true;
//     options.Password.RequireNonAlphanumeric = false;
//     options.Password.RequiredLength = 6;
// })
// .AddEntityFrameworkStores<AllocatrDbContext>()
// .AddDefaultTokenProviders();

// // ----------------- Configure Cookie -----------------
// builder.Services.ConfigureApplicationCookie(options =>
// {
//     options.LoginPath = "/api/auth/login"; // redirect if unauthorized
//     options.LogoutPath = "/api/auth/logout";
//     options.Cookie.HttpOnly = true;
//     options.Cookie.SameSite = SameSiteMode.Lax; // or None for cross-origin
// });

// // ----------------- Email Verification -----------------
// builder.Services.Configure<EmailSettings>(
//     builder.Configuration.GetSection(
//         EmailSettings.SectionName
//     )
// );

// builder.Services.Configure<DataProtectionTokenProviderOptions>(
//     options =>
//     {
//         options.TokenLifespan =
//             TimeSpan.FromHours(24);
//     }
// );



// // ----------------- Controllers -----------------
// // builder.Services.AddControllers();
// builder.Services
//     .AddControllers()
//     .AddJsonOptions(options =>
//     {
//         options.JsonSerializerOptions.Converters.Add(
//             new JsonStringEnumConverter()
//         );
//     });

// builder.Services.AddAuthorization();

// builder.Services.AddSingleton<SupabaseService>();

// builder.Services.AddProblemDetails();
// builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// builder.Services.AddScoped<ProfileService>();
// builder.Services.AddScoped<ProjectService>();
// builder.Services.AddScoped<ProjectAccessService>();
// builder.Services.AddScoped<AllocatProfileService>();
// builder.Services.AddScoped<TaskService>();
// builder.Services.AddScoped<TaskCommentService>();
// builder.Services.AddScoped<SkillCategoryService>();
// builder.Services.AddScoped<SkillService>();
// builder.Services.AddScoped<ProjectAllocatService>();
// builder.Services.AddScoped<IEmailService, EmailService>();
// builder.Services.AddScoped<CalendarService>();
// builder.Services.AddScoped<CalendarPlanningService>();
// builder.Services.AddScoped<ReviewService>();
// builder.Services.AddScoped<IFileStorageService, SupabaseFileStorageService>();
// builder.Services.AddScoped<UserFileService>();
// builder.Services.AddScoped<UserDocumentService>();

// var app = builder.Build();

// app.UseCors("AllowFrontend");
// app.UseAuthentication();
// app.UseAuthorization();
// app.UseExceptionHandler();

// app.MapControllers();

// app.MapGet("/", () => "Hello World!");

// // ----------------- Database seeder and auto migrations -----------------
// // using (var scope = app.Services.CreateScope())
// // {
// // 	var db = scope.ServiceProvider.GetRequiredService<AllocatrDbContext>();
// // 	await db.Database.MigrateAsync();
// // 	await DatabaseSeeder.SeedAsync(db);
// // }

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider
//         .GetRequiredService<AllocatrDbContext>();

//     await SkillCatalogSeeder.SeedAsync(db);
// }

// app.Run();


using AllocatrApi.Configuration;
using AllocatrApi.Data;
using AllocatrApi.Data.Seed;
using AllocatrApi.Infrastructure;
using AllocatrApi.Models;
using AllocatrApi.Services;
using AllocatrApi.Services.Storage;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

/* =========================================================
   CORS
========================================================= */

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    if (builder.Environment.IsDevelopment())
    {
        allowedOrigins = new[]
        {
            "http://localhost:5173"
        };
    }
    else
    {
        throw new InvalidOperationException(
            "Configure Cors:AllowedOrigins with the production frontend origin."
        );
    }
}

var validatedOrigins = allowedOrigins
    .Select(origin => origin?.Trim() ?? string.Empty)
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (validatedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Cors:AllowedOrigins must contain at least one valid frontend origin."
    );
}

foreach (var origin in validatedOrigins)
{
    if (
        !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
        (
            uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps
        ) ||
        !string.IsNullOrEmpty(uri.UserInfo) ||
        uri.AbsolutePath != "/" ||
        !string.IsNullOrEmpty(uri.Query) ||
        !string.IsNullOrEmpty(uri.Fragment) ||
        origin.Contains('*') ||
        origin.EndsWith('/')
    )
    {
        throw new InvalidOperationException(
            "Each Cors:AllowedOrigins value must be an exact HTTP or HTTPS origin without a path, wildcard or trailing slash."
        );
    }

    if (
        !builder.Environment.IsDevelopment() &&
        uri.Scheme != Uri.UriSchemeHttps
    )
    {
        throw new InvalidOperationException(
            "Production frontend origins must use HTTPS."
        );
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(validatedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

/* =========================================================
   DATABASE
========================================================= */

var connectionString = builder.Configuration
    .GetConnectionString("Migrations");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "The Migrations database connection string is not configured."
    );
}

builder.Services.AddDbContext<AllocatrDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

/* =========================================================
   IDENTITY
========================================================= */

builder.Services
    .AddIdentity<AllocatrUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

    })
    .AddEntityFrameworkStores<AllocatrDbContext>()
    .AddDefaultTokenProviders();

/* =========================================================
   AUTHENTICATION COOKIE
========================================================= */

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";

    options.Cookie.HttpOnly = true;

    options.Cookie.SameSite = SameSiteMode.Lax;

    options.Cookie.SecurePolicy =
        builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    };
});

/* =========================================================
   EMAIL VERIFICATION
========================================================= */

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(
        EmailSettings.SectionName
    )
);

builder.Services.Configure<DataProtectionTokenProviderOptions>(
    options =>
    {
        options.TokenLifespan = TimeSpan.FromHours(24);
    }
);

/* =========================================================
   CONTROLLERS
========================================================= */

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddAuthorization();

/* =========================================================
   ERROR HANDLING
========================================================= */

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

/* =========================================================
   SERVICES
========================================================= */

builder.Services.AddSingleton<SupabaseService>();

builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<ProjectAccessService>();
builder.Services.AddScoped<AllocatProfileService>();

builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TaskCommentService>();

builder.Services.AddScoped<SkillCategoryService>();
builder.Services.AddScoped<SkillService>();

builder.Services.AddScoped<ProjectAllocatService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<CalendarPlanningService>();

builder.Services.AddScoped<ReviewService>();

builder.Services.AddScoped<IFileStorageService, SupabaseFileStorageService>();

builder.Services.AddScoped<UserFileService>();
builder.Services.AddScoped<UserDocumentService>();

builder.Services.AddScoped<ProjectFileValidationService>();
builder.Services.AddScoped<ProjectFileService>();

builder.Services.Configure<AllocatrApi.Services.ProjectThumbnailOptions>(
    builder.Configuration.GetSection("ProjectThumbnails"));
builder.Services.AddSingleton<AllocatrApi.Services.ProjectThumbnailCache>();
builder.Services.AddSingleton<AllocatrApi.Services.ProjectThumbnailEncoder>();
builder.Services.AddScoped<AllocatrApi.Services.ProjectThumbnailService>();
builder.Services.AddHttpClient("ProjectThumbnailSource", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false,
    UseCookies = false,
});

/* =========================================================
   APPLICATION
========================================================= */

var app = builder.Build();

// Register early to catch errors from downstream middleware.
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

/* =========================================================
   SKILL CATALOGUE SEEDING
========================================================= */

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AllocatrDbContext>();

    await SkillCatalogSeeder.SeedAsync(db);
}

app.Run();