using System.Data;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.Queries;

public class GetDepartmentRootsHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly HybridCache _cache;

    public GetDepartmentRootsHandlerDapper(
        IDbConnectionFactory connectionFactory,
        HybridCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<GetDepartmentRootsDto?> Handle(GetDepartmentRootsRequest query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters();
        
        int prefetch = query.Prefetch;
        parameters.Add("prefetch", prefetch, DbType.Int32);
        
        var pagination = query.Pagination;
        parameters.Add("offset", (pagination.Page - 1) * pagination.PageSize, DbType.Int32);
        parameters.Add("page_size", pagination.PageSize, DbType.Int32);
        
        string cacheKey = $"departmentRoots_prefetch_{prefetch}_page_{pagination.Page}_pageSize_{pagination.PageSize}";
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5),
        };
        
        var departments = await _cache.GetOrCreateAsync<IEnumerable<DepartmentRootsDto>>(
            cacheKey,
            async cancel => await connection.QueryAsync<DepartmentRootsDto, bool, DepartmentRootsDto>(
                $"""
                 WITH roots AS (SELECT d.department_id,
                                       d.name,
                                       d.identifier,
                                       d.parent_id,
                                       d.path,
                                       d.depth,
                                       d.is_active,
                                       d.created_at,
                                       d.updated_at
                                FROM departments d
                                WHERE d.parent_id IS NULL
                                ORDER BY d.created_at
                                LIMIT @page_size OFFSET @offset)
                 
                 SELECT *,
                        (EXISTS(SELECT 1 FROM departments WHERE parent_id = roots.department_id OFFSET @prefetch LIMIT 1)) AS has_more_children
                 FROM roots
                 
                 UNION ALL
                 
                 SELECT c.*,
                        (EXISTS(SELECT 1 FROM departments WHERE parent_id = c.department_id)) AS has_more_children
                 FROM roots r 
                     CROSS JOIN LATERAL (SELECT d.department_id,
                                                         d.name,
                                                         d.identifier,
                                                         d.parent_id,
                                                         d.path,
                                                         d.depth,
                                                         d.is_active,
                                                         d.created_at,
                                                         d.updated_at
                                                  FROM departments d
                                                  WHERE d.parent_id = r.department_id
                                                    AND d.is_active = true
                                                  ORDER BY d.created_at
                                                  LIMIT @prefetch) c;
                 """,
                param: parameters,
                splitOn: "has_more_children",
                map: (department, hasMoreChildren) =>
                {
                    department.HasMoreChildren = hasMoreChildren;
                    return department;
                }),
            options,
            cancellationToken: cancellationToken);
        
        return new GetDepartmentRootsDto(departments.ToList());
    }
}