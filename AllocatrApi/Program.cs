using AllocatrApi.Dtos;
using AllocatrApi.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


List<ProjectDto> projects = SeedData.Projects;
Console.WriteLine(projects);

app.MapGet("/", () => "Hello World!");

app.MapGet("projects", () => projects);

app.MapGet("projects/{id}", (string id) => projects.Find(project => project.Id == id));





app.Run();
