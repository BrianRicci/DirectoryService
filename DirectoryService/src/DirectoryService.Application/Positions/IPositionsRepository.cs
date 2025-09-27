using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Guid> AddAsync(Position position, CancellationToken cancellationToken);
    
    Task<bool> IsNameExistsAsync(PositionName name, CancellationToken cancellationToken);
}