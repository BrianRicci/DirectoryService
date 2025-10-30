using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class PositionsRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly IDbConnectionFactory _connectionFactory;

    public PositionsRepository(
        DirectoryServiceDbContext dbContext,
        IDbConnectionFactory connectionFactory)
    {
        _dbContext = dbContext;
        _connectionFactory = connectionFactory;
    }
    
    public async Task<Result<Guid, Errors>> AddAsync(Position position, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Positions.AddAsync(position, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        
            return position.Id.Value;
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid().ToErrors();
        }
    }
    
    public async Task<bool> IsNameExistsAsync(PositionName name, CancellationToken cancellationToken)
    {
        bool isNameExists = await _dbContext.Positions
            .AnyAsync(p => p.Name == name && p.IsActive, cancellationToken);
        
        return isNameExists;
    }
    
    public async Task<Result<Position, Errors>> GetByIdAsync(PositionId positionId, CancellationToken cancellationToken)
    {
        var position = await _dbContext.Positions
            .Include(p => p.DepartmentPositions)
            .FirstOrDefaultAsync(p => p.Id == positionId, cancellationToken);
        
        if (position is null)
            return GeneralErrors.NotFound(positionId.Value).ToErrors();

        return position;
    }
    
    // метод возвращает null если не найдена позиция или найдено несколько позиций связанных с 1 департаментом
    // сделано для того, чтобы не пришлось вытаскивать весь список позиций в хэндлер, которые по итогу не будут использоваться
     public async Task<Result<GetPositionsToDeleteDto, Error>> GetPositionsRelatedToDepartmentAsync(
         DepartmentId departmentId, CancellationToken cancellationToken)
     {
         using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

         var positionDtos = 
             await connection.QueryAsync<GetPositionToDeleteDto, long, GetPositionToDeleteDto>(
             """
             WITH related_position_ids AS (
                 SELECT p.position_id
                 FROM positions p
                     JOIN department_positions dp ON p.position_id = dp.position_id 
                         AND dp.department_id = @departmentId
             )

             SELECT p.*, COUNT(p.position_id) AS count
             FROM positions p
                JOIN department_positions dp ON p.position_id = dp.position_id
                JOIN departments d ON dp.department_id = d.department_id
             WHERE dp.position_id IN (SELECT position_id FROM related_position_ids)
             AND d.is_active = true
             GROUP BY p.position_id;
             """,
             param: new { departmentId = departmentId.Value, },
             splitOn: "count",
             map: (position, count) =>
             {
                 position.Count = count;
                 return position;
             });

         var result = positionDtos.Where(dto => dto.Count == 1).ToList();
         
         return new GetPositionsToDeleteDto(result);
     }
}