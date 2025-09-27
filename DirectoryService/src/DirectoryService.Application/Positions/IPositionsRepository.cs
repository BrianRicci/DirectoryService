using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Guid> AddAsync(Position position, CancellationToken cancellationToken);
    
    Task<bool> IsNameExistsAsync(PositionName name, CancellationToken cancellationToken);

    Task<Result<Position, Errors>> GetByIdAsync(PositionId positionId, CancellationToken cancellationToken);
}