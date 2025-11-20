using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken);
    
    Task<Result<Position, Error>> GetByIdAsync(PositionId positionId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> SoftDeletePositionsRelatedToDepartmentAsync(
        DepartmentId departmentId, CancellationToken cancellationToken);
    
    Task<UnitResult<Error>> DeleteInactiveAsync(CancellationToken cancellationToken);

    Task<bool> IsNameExistsAsync(PositionName name, CancellationToken cancellationToken);
}