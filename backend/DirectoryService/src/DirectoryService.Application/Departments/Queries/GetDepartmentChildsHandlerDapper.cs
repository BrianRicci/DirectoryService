using System.Data;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace DirectoryService.Application.Departments.Queries;

public class GetDepartmentChildsHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly HybridCache _cache;

    public GetDepartmentChildsHandlerDapper(
        IDbConnectionFactory connectionFactory,
        HybridCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }
    
    public async Task<GetDepartmentChildsDto> Handle(GetDepartmentChildsRequest query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters();
        
        parameters.Add("parent_id", query.ParentId, DbType.Guid);
        
        var pagination = query.Pagination;
        parameters.Add("offset", (pagination.Page - 1) * pagination.PageSize, DbType.Int32);
        parameters.Add("page_size", pagination.PageSize, DbType.Int32);
        
        string cacheKey = $"{Constants.DEPARTMENT_CACHE_KEY}Childs_parentId_{query.ParentId}_page_{pagination.Page}_pageSize_{pagination.PageSize}";
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(5),
        };

        var departments = await _cache.GetOrCreateAsync<IEnumerable<DepartmentChildsDto>>(
            cacheKey,
            async cancel => await connection.QueryAsync<DepartmentChildsDto>(
                """
                SELECT d.department_id,
                       d.name,
                       d.identifier,
                       d.parent_id,
                       d.path,
                       d.depth,
                       d.is_active,
                       d.created_at,
                       d.updated_at,
                       (EXISTS(SELECT 1 FROM departments WHERE parent_id = d.department_id)) AS has_more_children
                FROM departments d
                WHERE d.is_active = true
                AND d.parent_id = @parent_id
                LIMIT @page_size OFFSET @offset
                """,
                param: parameters),
            options,
            cancellationToken: cancellationToken); 
        
        return new GetDepartmentChildsDto(departments.ToList());
    }
}