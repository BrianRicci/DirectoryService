using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Position position, CancellationToken cancellationToken);
    
    Task<bool> IsNameExistsAsync(PositionName name, CancellationToken cancellationToken);

    Task<Result<Position, Errors>> GetByIdAsync(PositionId positionId, CancellationToken cancellationToken);
    
    Task<UnitResult<Error>> SoftDeletePositionsRelatedToDepartmentAsync(
        DepartmentId departmentId, CancellationToken cancellationToken);
}