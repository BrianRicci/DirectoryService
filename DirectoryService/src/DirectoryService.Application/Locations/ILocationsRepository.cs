using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken);
    
    Task<bool> IsAddressExistsAsync(LocationAddress address, CancellationToken cancellationToken);
    
    Task<bool> IsAllExistsAsync(List<LocationId> locationIds, CancellationToken cancellationToken);

    Task<Result<GetLocationsToDeleteDto, Error>> GetLocationsRelatedToDepartmentAsync(
        DepartmentId departmentId, CancellationToken cancellationToken);
}