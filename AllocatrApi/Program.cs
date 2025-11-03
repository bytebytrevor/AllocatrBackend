using AllocatrApi.Dtos;
using AllocatrApi.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


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

}).WithName("GetProject");

// POST /projects
app.MapPost("projects", (CreateProjectDto newProject) =>
{
	ProjectDto project = new(
		"7f2c4b31-9d1a-4c71-b7ad-64b8e5f529d8",
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

	projects.Add(project);

	return Results.CreatedAtRoute("GetProject", new { id = project.Id}, project);
});

// POST /project/1
app.MapPut("projects/{id}", (string id, UpdateProjectDto updatedProject) =>
{
	int index = projects.FindIndex(project => project.Id == id);

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
});


// DELETE /projects/1
app.MapDelete("projects/{id}", (string id) =>
{
	projects.RemoveAll(project => project.Id.Equals(id));
});

app.Run();