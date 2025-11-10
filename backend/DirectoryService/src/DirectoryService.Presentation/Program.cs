using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.Command.Create;
using DirectoryService.Application.Departments.Command.Delete;
using DirectoryService.Application.Departments.Command.DeleteInactive;
using DirectoryService.Application.Departments.Command.Move;
using DirectoryService.Application.Departments.Command.Update;
using DirectoryService.Application.Departments.Queries;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.Command.Create;
using DirectoryService.Application.Locations.Queries;
using DirectoryService.Application.Positions;
using DirectoryService.Application.Positions.Create;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.BackgroundServices;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Repositories;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

string environment = builder.Environment.EnvironmentName;

builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") 
             ?? throw new ArgumentNullException("Seq"))
    .CreateLogger();

builder.Services.AddProgramDependencies(builder.Configuration);

var app = builder.Build();

app.UseExceptionMiddleware();

app.UseSerilogRequestLogging();

app.MapOpenApi();

app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService V1"));

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}