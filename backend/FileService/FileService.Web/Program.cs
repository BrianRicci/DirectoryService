using System.Globalization;
using FileService.Infrastructure.Postgres;
using FileService.Web.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    var environment = builder.Environment;

    builder.Configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true);

    builder.Configuration.AddEnvironmentVariables();

    builder.Services.AddConfiguration(builder.Configuration);

    WebApplication app = builder.Build();

    app.Configure();

    bool autoMigrate = builder.Configuration.GetSection("Database").GetValue<bool>("AutoMigrate");
    if (autoMigrate && environment.IsDevelopment())
    {
         using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();
            context.Database.Migrate();
        }
    }
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}