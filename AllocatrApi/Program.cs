using AllocatrApi.Data;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------- CORS -----------------
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://localhost:5173")
			  .AllowAnyMethod()
			  .AllowAnyHeader()
			  .AllowCredentials();
	});
});

// ----------------- DB Context -----------------
builder.Services.AddDbContext<AllocatrDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("Migrations"))
);

// ----------------- Identity -----------------
builder.Services.AddIdentity<AllocatrUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AllocatrDbContext>()
.AddDefaultTokenProviders();

// ----------------- Configure Cookie -----------------
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/api/auth/login"; // redirect if unauthorized
	options.LogoutPath = "/api/auth/logout";
	options.Cookie.HttpOnly = true;
	options.Cookie.SameSite = SameSiteMode.Lax; // or None for cross-origin
});

// ----------------- Controllers -----------------
builder.Services.AddControllers();
builder.Services.AddAuthorization();

builder.Services.AddSingleton<SupabaseService>();

builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<TaskService>();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();