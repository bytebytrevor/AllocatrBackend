using AllocatrApi.Data;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using AllocatrApi.Configuration;

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
builder.Services.AddIdentity<AllocatrUser, IdentityRole<Guid>>(options =>
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

// ----------------- Email Verification -----------------
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(
        EmailSettings.SectionName
    )
);

builder.Services.Configure<DataProtectionTokenProviderOptions>(
    options =>
    {
        options.TokenLifespan =
            TimeSpan.FromHours(24);
    }
);



// ----------------- Controllers -----------------
// builder.Services.AddControllers();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });
	
builder.Services.AddAuthorization();

builder.Services.AddSingleton<SupabaseService>();

builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<AllocatProfileService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TaskCommentService>();
builder.Services.AddScoped<SkillCategoryService>();
builder.Services.AddScoped<SkillService>();
builder.Services.AddScoped<ProjectAllocatService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

// ----------------- Database seeder and auto migrations -----------------
// using (var scope = app.Services.CreateScope())
// {
// 	var db = scope.ServiceProvider.GetRequiredService<AllocatrDbContext>();
// 	await db.Database.MigrateAsync();
// 	await DatabaseSeeder.SeedAsync(db);
// }

app.Run();