using Framework.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DirectoryService.Presentation.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseCors(corsPolicyBuilder =>
        {
            corsPolicyBuilder.WithOrigins(
                    app.Configuration.GetValue<string>("CorsAllowedOrigins:React", "http://localhost:3000"))
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });

        app.UseExceptionMiddleware();

        app.UseSerilogRequestLogging();

        app.MapOpenApi();

        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService V1"));

        app.MapControllers();

        return app;
    }
}