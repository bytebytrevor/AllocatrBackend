using AllocatrApi.Data;
using AllocatrApi.Dtos;
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

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");



// POST /projects


// // PUT /project/1
// app.MapPut("projects/{id}", (Guid id, UpdateProjectDto updatedProject) =>
// {
// 	int index = projects.FindIndex(project => project.Id == id);
// 	Console.WriteLine(index);
// 	if (index < 0)
// 		return Results.NotFound();

// 	projects[index] = new ProjectDto(
// 		updatedProject.Id,
// 		updatedProject.ProjectCode,
// 		updatedProject.Title,
// 		updatedProject.Description,
// 		updatedProject.Category,
// 		updatedProject.Tags,
// 		updatedProject.CreatedAt,
// 		updatedProject.UpdatedAt,
// 		updatedProject.StartDate,
// 		updatedProject.DueDate,
// 		updatedProject.Status,
// 		updatedProject.Progress,
// 		updatedProject.Priority,
// 		updatedProject.UserId,
// 		updatedProject.Allocats,
// 		updatedProject.TasksCount,
// 		updatedProject.MessagesCount,
// 		updatedProject.LastActivity,
// 		updatedProject.IsPublic,
// 		updatedProject.AllowBids,
// 		updatedProject.Budget,
// 		updatedProject.Currency,
// 		updatedProject.Attachments
// 	);

// 	return Results.NoContent();
// });

// // DELETE /projects/1
// app.MapDelete("projects/{id}", (string id) =>
// {
// 	projects.RemoveAll(project => project.Id.Equals(id));

// 	return Results.NoContent();
// });

// // GET /projects/projectId/tasks
// app.MapGet("projects/{projectId}/tasks", (Guid projectId) =>
// {
// 	var projectTasks = tasks
// 		.Where(t => t.ProjectId == projectId)
// 		.ToList();

// 	return projectTasks;
// });

// // PUT /projects/projectId/allocats/find

// app.MapPut("projects/{projectId}/allocats/{allocatId}", (Guid projectId, Guid allocatId) =>
// {
// 	var project = projects.FirstOrDefault(project => project.Id == projectId);

// 	if (project == null)
// 		return Results.NotFound();

// 	if (!project.AllocatIds.Contains(allocatId))
// 	{
// 		project.AllocatIds.Add(allocatId);
// 	}

// 	return Results.NoContent();
// });




// // GET /tasks
// app.MapGet("tasks", () => tasks);

// // GET /tasks/1
// app.MapGet("tasks/{id}", (Guid id) =>
// {
// 	var task = tasks.Find(t => t.Id == id);
// 	return task;
// });


// app.MapGet("allocats/{allocatId}", (Guid allocatId) =>
// {

// });

app.Run();