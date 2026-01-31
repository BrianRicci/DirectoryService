using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class PositionsRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<PositionsRepository> _logger;

    public PositionsRepository(
        DirectoryServiceDbContext dbContext,
        ILogger<PositionsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Positions.AddAsync(position, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return position.Id.Value;
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid();
        }
    }

    public async Task<Result<Position, Error>> GetByIdAsync(PositionId positionId, CancellationToken cancellationToken)
    {
        var position = await _dbContext.Positions
            .Include(p => p.DepartmentPositions)
            .FirstOrDefaultAsync(p => p.Id == positionId && p.IsActive, cancellationToken);

        if (position is null)
            return GeneralErrors.NotFound(positionId.Value);

        return position;
    }
    
    public async Task<Result<Position, Error>> GetByIdWithLock(PositionId positionId, CancellationToken cancellationToken)
    {
        var position = (await _dbContext.Positions
                .FromSqlInterpolated(
                    $"""
                      SELECT position_id,
                             name,
                             description,
                             created_at,
                             updated_at,
                             deleted_at,
                             is_active
                      FROM positions 
                      WHERE position_id = {positionId.Value} AND is_active = true 
                      FOR UPDATE
                     """)
                .ToListAsync(cancellationToken))
            .FirstOrDefault();

        if (position is null)
            return GeneralErrors.NotFound(positionId.Value);

        return position;
    }

    public async Task<UnitResult<Error>> SoftDeletePositionsRelatedToDepartmentAsync(
        DepartmentId departmentId, CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"""
             WITH 
                 relation_position_ids AS (
                     SELECT p.position_id
                     FROM positions p
                       JOIN department_positions dp ON p.position_id = dp.position_id
                           AND dp.department_id = {departmentId.Value}
                       JOIN departments d ON dp.department_id = d.department_id
                     WHERE d.is_active = true
                 ),
                 
                 positions_count AS (
                     SELECT dp.position_id, COUNT(dp.position_id) AS count
                     FROM department_positions dp
                       JOIN relation_position_ids rp ON rp.position_id = dp.position_id
                     GROUP BY dp.position_id
                 ),
                 
                 positions_to_delete AS (
                     SELECT pc.position_id
                     FROM positions_count pc
                     WHERE pc.count = 1
                 )

             UPDATE positions p
             SET
                 is_active = false,
                 updated_at = {DateTime.UtcNow},
                 deleted_at = {DateTime.UtcNow}
             WHERE p.position_id in (SELECT * FROM positions_to_delete) 
             """, cancellationToken);

        return UnitResult.Success<Error>();
    }
    
    public async Task<UnitResult<Error>> DeleteInactiveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"""
             DELETE FROM positions
             WHERE is_active = false
                 AND NOT EXISTS (
                     SELECT 1
                     FROM department_positions
                     WHERE position_id = positions.position_id)
             """, cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<bool> IsNameExistsAsync(PositionName name, CancellationToken cancellationToken)
    {
        bool isNameExists = await _dbContext.Positions
            .AnyAsync(p => p.Name == name && p.IsActive, cancellationToken);

        return isNameExists;
    }
    
    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save changes");
            return GeneralErrors.Failure("Failed to save changes");
        }
    }
}