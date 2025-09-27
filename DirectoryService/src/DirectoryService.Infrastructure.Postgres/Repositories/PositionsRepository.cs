using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class PositionsRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    
    public PositionsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Guid> AddAsync(Position position, CancellationToken cancellationToken)
    {
        await _dbContext.Positions.AddAsync(position, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return position.Id.Value;
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
}