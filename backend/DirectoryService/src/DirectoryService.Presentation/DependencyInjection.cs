using DirectoryService.Application;
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
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared;
using Errors = Shared.Errors;

namespace DirectoryService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddWebDependencies()
            .AddApplication()
            .AddInfrastructureServices(configuration)
            .AddDistributedCache(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Directory Service",
                Version = "v1",
                Contact = new OpenApiContact
                {
                    Name = "Yunus", Email = "yunus.ibragimov49@gmail.com",
                },
            });
        });
        
        services.AddSerilog();

        return services;
    }
    
    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DirectoryServiceDbContext>(_ =>
            new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(_ =>
            new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        services.AddScoped<IPositionsRepository, PositionsRepository>();

        services.AddScoped<IValidator<CreateLocationCommand>, CreateLocationValidator>();
        services.AddScoped<IValidator<CreateDepartmentCommand>, CreateDepartmentValidator>();
        services.AddScoped<IValidator<CreatePositionCommand>, CreatePositionValidator>();
        services.AddScoped<IValidator<UpdateDepartmentLocationsCommand>, UpdateDepartmentLocationsValidator>();
        services.AddScoped<IValidator<MoveDepartmentCommand>, MoveDepartmentValidator>();
        services.AddScoped<IValidator<DeleteDepartmentCommand>, DeleteDepartmentValidator>();
        
        services.AddScoped<CreateLocationHandler>();
        services.AddScoped<CreateDepartmentHandler>();
        services.AddScoped<CreatePositionHandler>();
        services.AddScoped<UpdateDepartmentLocationsHandler>();
        services.AddScoped<MoveDepartmentHandler>();
        services.AddScoped<GetLocationByIdHandler>();
        services.AddScoped<GetByIdHandlerDapper>();
        services.AddScoped<GetLocationsHandlerDapper>();
        services.AddScoped<GetDepartmentsTopHandlerDapper>();
        services.AddScoped<GetDepartmentRootsHandlerDapper>();
        services.AddScoped<GetDepartmentChildsHandlerDapper>();
        services.AddScoped<DeleteDepartmentHandler>();
        services.AddScoped<DeleteInactiveHandler>();
        
        services.AddHostedService<InactiveDepartmentsCleanerBackgroundService>();

        return services;
    }
    
    private static IServiceCollection AddDistributedCache(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            string connection = configuration.GetConnectionString("Redis") 
                                ?? throw new ArgumentNullException(nameof(connection));

            options.Configuration = connection;
        });
        
        services.AddHybridCache();

        return services;
    }
}