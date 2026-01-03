using System.Data.Common;
using Amazon.S3;
using FileService.Core;
using FileService.Infrastructure.Postgres;
using FileService.Infrastructure.S3;
using FileService.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Npgsql;
using Respawn;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace FileService.IntegrationTests.Infrastructure;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("file_service_db_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
    
    private readonly MinioContainer _minioContainer = new MinioBuilder()
        .WithImage("minio/minio")
        .WithUsername("minioadmin")
        .WithPassword("minioadmin")
        .Build();
    
    private Respawner _respawner = null!;
    
    private DbConnection _dbConnection = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _minioContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        FileServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        
        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        await InitializeRespawner();
    }
    
    public new async Task DisposeAsync()
    {
        await _minioContainer.StopAsync();
        await _minioContainer.DisposeAsync();
        
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
    
    public async Task ResetDatabeseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Tests.json"), optional: true);
        });
        
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<FileServiceDbContext>();
            services.RemoveAll<IReadDbContext>();

            services.AddScoped<FileServiceDbContext>(_ => 
                new FileServiceDbContext(_dbContainer.GetConnectionString()!));
        
            services.AddScoped<IReadDbContext, FileServiceDbContext>(_ => 
                new FileServiceDbContext(_dbContainer.GetConnectionString()!));

            services.RemoveAll<IAmazonS3>();
            
            services.AddSingleton<IAmazonS3>(sp =>
            {
                S3Options s3Options = sp.GetRequiredService<IOptions<S3Options>>().Value;

                ushort minioPort = _minioContainer.GetMappedPublicPort(9000);
                
                var config = new AmazonS3Config { 
                    ServiceURL = $"http://{_minioContainer.Hostname}:{minioPort}",
                    UseHttp = true,
                    ForcePathStyle = true,
                };
                
                return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
            });
        });
    }
    
    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
            });
    }
}