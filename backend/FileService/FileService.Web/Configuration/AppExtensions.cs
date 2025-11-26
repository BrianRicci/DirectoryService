using FileService.Infrastructure.Postgres;
using Framework.Endpoints;
using Framework.Middlewares;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FileService.Web.Configuration;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        app.UseRequestCorrelationId();
        app.UseSerilogRequestLogging();

        app.MapOpenApi();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "MediaAsset Service V1");
        });

        RouteGroupBuilder apiGroup = app.MapGroup("/api").WithOpenApi();
        app.MapEndpoints(apiGroup);

        app.UseAutoMigrate();
        
        return app;
    }

    private static IApplicationBuilder UseAutoMigrate(this WebApplication app)
    {
        var environment = app.Environment;
        
        bool autoMigrate = app.Configuration.GetSection("Database").GetValue<bool>("AutoMigrate");
        if (autoMigrate && environment.IsDevelopment())
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
                context.Database.Migrate();
            }
        }

        return app;
    }
}