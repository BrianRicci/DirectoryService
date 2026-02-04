using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.Queries;

public class GetDepartmentNamesForFilterDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentNamesForFilterDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<PaginationResponse<DepartmentNamesDto>, Errors>> Handle(
        GetDepartmentNamesRequest query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();

        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            conditions.Add("name ILIKE @search");
            parameters.Add("search", $"%{query.Search}%");
        }
        
        conditions.Add("is_active = true");
        
        string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        long? totalCount = 0;
        
        long departmentNamesCount = 0;
        
        var departmentNames = await connection.QueryAsync<DepartmentNamesDto, long, long, DepartmentNamesDto>(
                $"""
                 WITH departments_stats AS (
                     SELECT COUNT(*) as count
                     FROM departments
                 )
                 
                 SELECT d.department_id,
                        d.name,
                        COUNT(*) OVER () AS total_count,
                        ds.count as departments_stats 
                 FROM departments d
                        CROSS JOIN departments_stats ds
                 {whereClause}
                 """,
                param: parameters,
                splitOn: "total_count, departments_stats",
                map: (departmentName, isRequestCount, allDepartmentsCount) =>
                {
                    totalCount = isRequestCount;
                    
                    departmentNamesCount = allDepartmentsCount;
                    
                    return new DepartmentNamesDto(departmentName.DepartmentId, departmentName.Name);
                });
        
        long totalPages = (long)Math.Ceiling(departmentNamesCount / (double)query.PageSize);

        return new PaginationResponse<DepartmentNamesDto>(departmentNames.ToList(), totalCount ?? 0, query.Page, query.PageSize, totalPages);
    }
}