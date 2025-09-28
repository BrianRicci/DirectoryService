using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken);
    
    Task<bool> IsAddressExistsAsync(LocationAddress address, CancellationToken cancellationToken);
    
    Task<bool> IsAllExistsAsync(List<LocationId> locationIds, CancellationToken cancellationToken);
}