using FileService.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests.Infrastructure;

public class FileServiceTestsBase : IClassFixture<IntegrationTestsWebFactory>, IAsyncLifetime
{
    public const string TEST_FILE_NAME = "test-file.mp4";
    
    protected FileServiceTestsBase(IntegrationTestsWebFactory factory)
    {
        AppHttpClient = factory.CreateClient();
        HttpClient = new HttpClient();
        Services = factory.Services;
    }

    protected IServiceProvider Services { get; init; }

    protected HttpClient AppHttpClient { get; init; }

    protected HttpClient HttpClient { get; init; }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task ExecuteInDb(Func<FileServiceDbContext, Task> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();

        FileServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<FileServiceDbContext>();

        await action(dbContext);
    }
}