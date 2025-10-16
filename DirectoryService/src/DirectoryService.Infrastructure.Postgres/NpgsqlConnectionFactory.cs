﻿using System.Data;
using DirectoryService.Application.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres;

public class NpgsqlConnectionFactory : IDbConnectionFactory, IDisposable, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    
    public NpgsqlConnectionFactory(IConfiguration configuration)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DirectoryServiceDb"));
        dataSourceBuilder.UseLoggerFactory(CreateLoggerFactory());
        
        _dataSource = dataSourceBuilder.Build();   
    }
    
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }
    
    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync();
    }

    private ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder => builder.AddConsole());
    }
}