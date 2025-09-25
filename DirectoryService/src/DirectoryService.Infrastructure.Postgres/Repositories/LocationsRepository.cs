using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public LocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id.Value;
    }

    public async Task<bool> IsAddressExistsAsync(LocationAddress address, CancellationToken cancellationToken)
    {
        bool isAddressExists = await _dbContext.Locations.AnyAsync(l => l.Address == address, cancellationToken);
        
        return isAddressExists;
    }
    
    public async Task<bool> IsAllLocationsExistsAsync(List<LocationId> locationIds, CancellationToken cancellationToken)
    {
        bool isAllLocationsExists = await _dbContext.Locations
            .Where(l => locationIds.Contains(l.Id))
            .CountAsync(cancellationToken) == locationIds.Count;
        
        return isAllLocationsExists;
    }
}