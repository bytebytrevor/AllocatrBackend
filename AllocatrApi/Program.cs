using AllocatrApi.Dtos;
using AllocatrApi.Data;
using Microsoft.AspNetCore.Cors.Infrastructure;

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

var app = builder.Build();

app.UseCors("_allowSpecificOrigins");



var GetProjectEndpointName = "Get Project";
List<ProjectDto> projects = SeedData.Projects;
Console.WriteLine(projects);

app.MapGet("/", () => "Hello World!");

// GET /projects
app.MapGet("projects", () => projects);

// GET /projects/1
app.MapGet("projects/{id}", (string id) =>
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
		Guid.NewGuid().ToString(),
		"ALC-0012",
		newProject.Title,
		newProject.Description,
		newProject.Type,
		newProject.Category,
		new DateTime(),
		newProject.UpdatedAt,
		newProject.StartDate,
		newProject.DueDate,
		"pending",
		0,
		newProject.Priority,
		newProject.UserId,
		newProject.Allocats,
		0,
		0,
		new DateTime(),
		newProject.IsPublic,
		newProject.AllowBids,
		newProject.Budget,
		newProject.Currency,
		newProject.Attachments
	);

	if (project == null)
		return Results.BadRequest();
	
	projects.Add(project);

	return Results.CreatedAtRoute(GetProjectEndpointName, new { id = project?.Id }, project);
});

// POST /project/1
app.MapPut("projects/{id}", (string id, UpdateProjectDto updatedProject) =>
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
		updatedProject.Type,
		updatedProject.Category,
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

app.Run();