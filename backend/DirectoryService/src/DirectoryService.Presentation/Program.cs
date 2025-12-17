using DirectoryService.Presentation;
using Framework.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

string environment = builder.Environment.EnvironmentName;

builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") 
             ?? throw new ArgumentNullException("Seq"))
    .CreateLogger();

builder.Services.AddProgramDependencies(builder.Configuration);

var app = builder.Build();

app.UseCors(builder =>
{
    builder.WithOrigins("http://localhost:3000")
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

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program;
}