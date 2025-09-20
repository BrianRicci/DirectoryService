using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Guid> AddAsync(Location location, CancellationToken cancellationToken);
    
    Task<Location?> GetByAddressAsync(LocationAddress address, CancellationToken cancellationToken);
    
    // Task<Guid> UpdateAsync(Location location, CancellationToken cancellationToken);
    //
    // Task<Guid> DeleteAsync(Guid locationId, CancellationToken cancellationToken);
    //
    // Task<Location> GetByIdAsync(Guid locationId, CancellationToken cancellationToken);
}