using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
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

    public async Task<Result<GetDepartmentNamesDto, Errors>> Handle(
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

        var departmentNames = await connection.QueryAsync<DepartmentNamesDto>(
                $"""
                 SELECT department_id, name 
                 FROM departments
                 {whereClause}
                 """,
                param: parameters);

        return new GetDepartmentNamesDto(departmentNames.ToList());
    }
}