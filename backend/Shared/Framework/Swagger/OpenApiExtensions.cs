using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Framework.Swagger;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiSpec(this IServiceCollection services, string title, string version)
    {
        services.AddOpenApi();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = title,
                Version = version,
                Contact = new OpenApiContact
                {
                    Name = "Yunus", Email = "yunus.ibragimov49@gmail.com"
                }
            });
        });

        return services;
    }
}