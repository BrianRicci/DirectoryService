using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
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
            LocationId = location.Id.Value,
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

public class GetByIdHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetByIdHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<GetLocationDto?> Handle(GetLocationByIdRequest query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        GetLocationDto? locationDto = null;

        await connection.QueryAsync<GetLocationDto, LocationAddressDto, DepartmentDto, GetLocationDto>(
            """
            SELECT l.location_id,
                   l.name, 
                   l.is_active,
                   l.timezone,
                   l.created_at,
                   l.updated_at,
                   l.country,
                   l.region,
                   l.city,
                   l.street,
                   l.house,
                   d.department_id,
                   d.name,
                   d.identifier,
                   d.parent_id,
                   d.path,
                   d.depth,
                   d.is_active,
                   d.created_at,
                   d.updated_at
            FROM locations l
            JOIN department_locations dl ON dl.location_id = l.location_id
            JOIN departments d ON d.department_id = dl.department_id
            WHERE l.location_id = @locationId
            """,
            param: new
            {
                locationId = query.LocationId,
            },
            splitOn: "country, department_id",
            map: (l, a, d) =>
            {
                if (locationDto is null)
                    locationDto = l;
                
                // address mapping
                locationDto.Address = a;
                
                // department mapping
                locationDto.Departments.Add(d);

                return locationDto;
            });
        
        return locationDto;
    }
}