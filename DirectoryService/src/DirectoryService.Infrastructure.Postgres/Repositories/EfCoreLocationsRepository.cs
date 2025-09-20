using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class EfCoreLocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public EfCoreLocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id.Value;
    }

    public async Task<Location?> GetByAddressAsync(LocationAddress address, CancellationToken cancellationToken)
    {
        Location? location = await _dbContext.Locations
            .Where(l => l.Address == address)
            .SingleOrDefaultAsync(cancellationToken);
        
        return location;
    }
}