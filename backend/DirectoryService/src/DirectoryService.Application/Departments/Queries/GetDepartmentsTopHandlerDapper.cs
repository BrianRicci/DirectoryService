using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace DirectoryService.Application.Departments.Queries;

public class GetDepartmentsTopHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly HybridCache _cache;

    public GetDepartmentsTopHandlerDapper(
        IDbConnectionFactory connectionFactory,
        HybridCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<GetDepartmentsTopDto?> Handle(CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        string cacheKey = $"{Constants.DEPARTMENT_CACHE_KEY}Childs_top_5";
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5),
        };
        
        var departments = await _cache.GetOrCreateAsync<IEnumerable<DepartmentsTopDto>>(
            cacheKey,
            async cancel => await connection.QueryAsync<DepartmentsTopDto>(
                $"""
                 SELECT d.department_id,
                        d.name,
                        d.identifier,
                        d.parent_id,
                        d.path,
                        d.depth,
                        d.is_active,
                        d.created_at,
                        d.updated_at,
                        COUNT(*) AS positions_count
                 FROM departments d
                 JOIN department_positions dp on d.department_id = dp.department_id
                 WHERE d.is_active = true 
                 GROUP BY d.department_id
                 ORDER BY positions_count DESC
                 LIMIT 5
                 """),
            options,
            cancellationToken: cancellationToken);
        
        return new GetDepartmentsTopDto(departments.ToList());
    }
}