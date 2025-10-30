using System.Data;
using DirectoryService.Application.Database;
using Npgsql;

namespace DirectoryService.IntegrationTests;

public class NpgsqlConnectionFactoryForTests : IDbConnectionFactory, IDisposable, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlConnectionFactoryForTests(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public void Dispose() => _dataSource.Dispose();

    public async ValueTask DisposeAsync() => await _dataSource.DisposeAsync();
}