using DirectoryService.Application.Departments;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateDepartment;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Positions;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Infrastructure.Postgres;
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

builder.Services.AddScoped<ITransactionManager, TransactionManager>();
builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
builder.Services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
builder.Services.AddScoped<IPositionsRepository, PositionsRepository>();

builder.Services.AddScoped<IValidator<CreateLocationCommand>, CreateLocationValidator>();
builder.Services.AddScoped<IValidator<CreateDepartmentCommand>, CreateDepartmentValidator>();
builder.Services.AddScoped<IValidator<CreatePositionCommand>, CreatePositionValidator>();
builder.Services.AddScoped<IValidator<UpdateDepartmentLocationsCommand>, UpdateDepartmentLocationsValidator>();
builder.Services.AddScoped<IValidator<MoveDepartmentCommand>, MoveDepartmentValidator>();

builder.Services.AddScoped<CreateLocationHandler>();
builder.Services.AddScoped<CreateDepartmentHandler>();
builder.Services.AddScoped<CreatePositionHandler>();
builder.Services.AddScoped<UpdateDepartmentLocationsHandler>();
builder.Services.AddScoped<MoveDepartmentHandler>();

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