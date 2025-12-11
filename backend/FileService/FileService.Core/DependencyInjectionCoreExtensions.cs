using FileService.Core.Command;
using FileService.Core.Queries;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Core;

public static class DependencyInjectionCoreExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjectionCoreExtensions).Assembly);

        services.AddScoped<StartMultipartUploadHandler>();
        services.AddScoped<GetChunkUploadHandler>();
        services.AddScoped<CompleteMultipartUploadHandler>();
        services.AddScoped<AbortMultipartUploadHandler>();
        services.AddScoped<GetMediaAssetInfoHandler>();
        services.AddScoped<GetMediaAssetsInfoHandler>();
        services.AddScoped<GetDownloadUrlHandler>();
        services.AddScoped<DeleteFileHandler>();
        
        return services;
    }
}