using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Queries;

public class GetDepartmentsTopHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDepartmentsTopHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetDepartmentsTopDto?> Handle(CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var departments = await connection.QueryAsync<DepartmentsTopDto>(
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
             """);
        
        return new GetDepartmentsTopDto(departments.ToList());
    }
}