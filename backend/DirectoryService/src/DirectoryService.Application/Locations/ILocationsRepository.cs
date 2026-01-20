using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken);
    
    Task<Result<Location, Error>> GetByIdAsync(LocationId locationId, CancellationToken cancellationToken);
    
    Task<Result<Location, Error>> GetByIdWithLock(LocationId locationId, CancellationToken cancellationToken);

    Task<Result<List<Location>, Error>> GetRelatedDepartmentsAsync(
        LocationId locationId, CancellationToken cancellationToken);
    
    Task<UnitResult<Error>> SoftDeleteLocationsRelatedToDepartmentAsync(
        DepartmentId departmentId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteInactiveAsync(CancellationToken cancellationToken);

    Task<bool> IsAddressExistsAsync(LocationAddress address, CancellationToken cancellationToken);

    Task<bool> IsAllExistsAsync(List<LocationId> locationIds, CancellationToken cancellationToken);
    
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}