using AllocatrApi.Dtos;
using AllocatrApi.Data;
using Microsoft.EntityFrameworkCore;
using AllocatrApi.Extensions;
using AllocatrApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Cors setup
var AllowSpecificOrigins = "_allowSpecificOrigins";

builder.Services.AddCors(options =>
{
	options.AddPolicy(name: AllowSpecificOrigins,
	policy =>
	{
		policy.WithOrigins(
			"http://localhost:5173"
		)
		.AllowAnyHeader()
		.AllowAnyMethod()
		.AllowCredentials();
	});
});

builder.Services.AddDbContext<AllocatrDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("Migrations")));

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddIdentityConfig();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddScoped<TokenService>();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.Events.OnRedirectToLogin = context =>
	{
		context.Response.StatusCode = StatusCodes.Status401Unauthorized;
		return Task.CompletedTask;
	};

	options.Events.OnRedirectToAccessDenied = context =>
	{
		context.Response.StatusCode = StatusCodes.Status403Forbidden;
		return Task.CompletedTask;
	};
});

var app = builder.Build();

app.UseCors("_allowSpecificOrigins");

if (!app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var GetProjectEndpointName = "Get Project";
List<ProjectDto> projects = SeedData.Projects;
List<TaskDto> tasks = TasksSeedData.Tasks;


app.MapGet("/", () => "Hello World!");

// GET /projects
app.MapGet("projects", () => projects);

// GET /projects/1
app.MapGet("projects/{id}", (Guid id) =>
{
	var project = projects.Find(project => project.Id == id);
	if (project == null)
		return Results.NotFound();

	return Results.Ok(project);

}).WithName(GetProjectEndpointName);



// POST /projects
app.MapPost("projects", (CreateProjectDto newProject) =>
{
	ProjectDto project = new(
		Guid.NewGuid(),
		"ALC-0012", // ProjectCode
		newProject.Title,
		newProject.Description,
		newProject.Category,
		newProject.Tags,
		new DateTime(), // CreatedAt
		new DateTime(), // UpdateAt
		newProject.StartDate,
		newProject.DueDate,
		"pending", // Status
		0, // Progress
		newProject.Priority,
		Guid.NewGuid(), // UserId ** Use auth 
		[], // AllocatIds
		0, // TaskCount
		0, // MessagesCount
		new DateTime(), // LastActivity
		newProject.IsPublic,
		newProject.AllowBids,
		newProject.Budget,
		newProject.Currency,
		[] // Attachments
	);

	if (project == null)
		return Results.BadRequest();

	projects.Add(project);

	return Results.CreatedAtRoute(GetProjectEndpointName, new { id = project?.Id }, project);
});

// PUT /project/1
app.MapPut("projects/{id}", (Guid id, UpdateProjectDto updatedProject) =>
{
	int index = projects.FindIndex(project => project.Id == id);
	Console.WriteLine(index);
	if (index < 0)
		return Results.NotFound();

	projects[index] = new ProjectDto(
		updatedProject.Id,
		updatedProject.ProjectCode,
		updatedProject.Title,
		updatedProject.Description,
		updatedProject.Category,
		updatedProject.Tags,
		updatedProject.CreatedAt,
		updatedProject.UpdatedAt,
		updatedProject.StartDate,
		updatedProject.DueDate,
		updatedProject.Status,
		updatedProject.Progress,
		updatedProject.Priority,
		updatedProject.UserId,
		updatedProject.Allocats,
		updatedProject.TasksCount,
		updatedProject.MessagesCount,
		updatedProject.LastActivity,
		updatedProject.IsPublic,
		updatedProject.AllowBids,
		updatedProject.Budget,
		updatedProject.Currency,
		updatedProject.Attachments
	);

	return Results.NoContent();
});

// DELETE /projects/1
app.MapDelete("projects/{id}", (string id) =>
{
	projects.RemoveAll(project => project.Id.Equals(id));

	return Results.NoContent();
});

// GET /projects/projectId/tasks
app.MapGet("projects/{projectId}/tasks", (Guid projectId) =>
{
	var projectTasks = tasks
		.Where(t => t.ProjectId == projectId)
		.ToList();

	return projectTasks;
});

// PUT /projects/projectId/allocats/find

app.MapPut("projects/{projectId}/allocats/{allocatId}", (Guid projectId, Guid allocatId) =>
{
	var project = projects.FirstOrDefault(project => project.Id == projectId);

	if (project == null)
		return Results.NotFound();

	if (!project.AllocatIds.Contains(allocatId))
	{
		project.AllocatIds.Add(allocatId);
	}

	return Results.NoContent();
});

app.Run();

/********************************* TASKS *********************************/
// GET /tasks
app.MapGet("tasks", () => tasks);

// GET /tasks/1
app.MapGet("tasks/{id}", (Guid id) =>
{
	var task = tasks.Find(t => t.Id == id);
	return task;
});

/********************************* ALLOCATS *********************************/
app.MapGet("allocats/{allocatId}", (Guid allocatId) =>
{

});