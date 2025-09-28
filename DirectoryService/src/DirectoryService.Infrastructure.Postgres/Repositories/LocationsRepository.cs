using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public LocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Errors>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return location.Id.Value;
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid().ToErrors();
        }
    }

    public async Task<bool> IsAddressExistsAsync(LocationAddress address, CancellationToken cancellationToken)
    {
        bool isAddressExists = await _dbContext.Locations.AnyAsync(l => l.Address == address, cancellationToken);
        
        return isAddressExists;
    }
    
    public async Task<bool> IsAllExistsAsync(List<LocationId> locationIds, CancellationToken cancellationToken)
    {
        bool isAllExists = await _dbContext.Locations
            .Where(l => locationIds.Contains(l.Id))
            .CountAsync(cancellationToken) == locationIds.Count;
        
        return isAllExists;
    }
}