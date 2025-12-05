using FileService.Core;
using FileService.Infrastructure.Postgres.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Infrastructure.Postgres;

public static class DependencyInjectionPostgresExtensions
{
    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<FileServiceDbContext>(_ => 
            new FileServiceDbContext(configuration.GetConnectionString("FileServiceDb")!));

        services.AddScoped<IMediaAssetsRepository, MediaAssetsRepository>();

        return services;
    }
}