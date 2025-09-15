using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts;
using DirectoryService.Infrastructure.Postgres;
using DirectoryService.Infrastructure.Postgres.Repositories;
using DirectoryService.Presentation;
using DirectoryService.Presentation.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProgramDependencies();

builder.Services.AddScoped<DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration.GetConnectionString("DirectoryServiceDb")!));

builder.Services.AddScoped<ILocationsRepository, EfCoreLocationsRepository>();

builder.Services.AddScoped<IValidator<CreateLocationDto>, CreateLocationValidator>();

builder.Services.AddScoped<CreateLocationHandler>();

var app = builder.Build();

app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();

app.Run();