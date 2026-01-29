using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Positions;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Queries;

public sealed class GetPositionByIdHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetPositionByIdHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<Result<GetPositionDto, Errors>> Handle(GetPositionByIdRequest query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        GetPositionDto? positionDto = null;

        await connection.QueryAsync<GetPositionDto, DepartmentDto, GetPositionDto>(
            """
            SELECT p.position_id,
                   p.name, 
                   p.is_active,
                   p.description,
                   p.created_at,
                   p.updated_at,
                   d.department_id,
                   d.name,
                   d.identifier,
                   d.parent_id,
                   d.path,
                   d.depth,
                   d.is_active,
                   d.created_at,
                   d.updated_at
            FROM positions p
            JOIN department_positions dp ON dp.position_id = p.position_id
            JOIN departments d ON d.department_id = dp.department_id
            WHERE p.position_id = @positionId
            """,
            param: new
            {
                positionId = query.PositionId,
            },
            splitOn: "department_id",
            map: (position, department) =>
            {
                if (positionDto is null)
                    positionDto = position;
                
                // department mapping
                positionDto.Departments.Add(department);

                return positionDto;
            });
        
        if (positionDto is null)
        {
            return GeneralErrors.NotFound(query.PositionId, "Position").ToErrors();
        }
        
        return positionDto;
    }
}