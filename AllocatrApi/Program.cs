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
app.MapGet("projects/{id}", (string id) => projects.Find(project => project.Id == id));

// POST /projects
app.MapPost("projects", (CreateProjectDto newProject) =>
{
	ProjectDto project = new(
		"7f2c4b31-9d1a-4c71-b7ad-64b8e5f529d8",
		"ALC-0011",
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
});



app.Run();
