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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") 
             ?? throw new ArgumentNullException("Seq"))
    .CreateLogger();

builder.Services.AddProgramDependencies();

builder.Services.AddScoped<DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddScoped<ITransactionManager, TransactionManager>();
builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
builder.Services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
builder.Services.AddScoped<IPositionsRepository, PositionsRepository>();

builder.Services.AddScoped<IValidator<CreateLocationCommand>, CreateLocationValidator>();
builder.Services.AddScoped<IValidator<CreateDepartmentCommand>, CreateDepartmentValidator>();
builder.Services.AddScoped<IValidator<CreatePositionCommand>, CreatePositionValidator>();
builder.Services.AddScoped<IValidator<UpdateDepartmentLocationsCommand>, UpdateDepartmentLocationsValidator>();
builder.Services.AddScoped<IValidator<MoveDepartmentCommand>, MoveDepartmentValidator>();
builder.Services.AddScoped<IValidator<DeleteDepartmentCommand>, DeleteDepartmentValidator>();

builder.Services.AddScoped<CreateLocationHandler>();
builder.Services.AddScoped<CreateDepartmentHandler>();
builder.Services.AddScoped<CreatePositionHandler>();
builder.Services.AddScoped<UpdateDepartmentLocationsHandler>();
builder.Services.AddScoped<MoveDepartmentHandler>();
builder.Services.AddScoped<GetLocationByIdHandler>();
builder.Services.AddScoped<GetByIdHandlerDapper>();
builder.Services.AddScoped<GetLocationsHandlerDapper>();
builder.Services.AddScoped<GetDepartmentsTopHandlerDapper>();
builder.Services.AddScoped<GetDepartmentRootsHandlerDapper>();
builder.Services.AddScoped<GetDepartmentChildsHandlerDapper>();
builder.Services.AddScoped<DeleteDepartmentHandler>();
builder.Services.AddScoped<DeleteInactiveHandler>();

builder.Services.AddHostedService<InactiveDepartmentsCleanerBackgroundService>();

var app = builder.Build();

app.UseExceptionMiddleware();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}