using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries;

public class GetByIdHandler
{
    private readonly IReadDbContext _readDbContext;

    public GetByIdHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }
    
    public async Task<GetLocationDto?> Handle(GetLocationByIdRequest query, CancellationToken cancellationToken)
    {
        var location = await _readDbContext.LocationsRead
            .FirstOrDefaultAsync(l => l.Id == new LocationId(query.LocationId), cancellationToken);

        if (location is null)
        {
            return null;
        }

        return new GetLocationDto()
        {
            Id = location.Id.Value,
            Name = location.Name.Value,
            Address = new LocationAddressDto(
                location.Address.Country,
                location.Address.Region, 
                location.Address.City,
                location.Address.Street,
                location.Address.House),
            Timezone = location.Timezone.Value,
            IsActive = location.IsActive,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt,
        };
    }
}