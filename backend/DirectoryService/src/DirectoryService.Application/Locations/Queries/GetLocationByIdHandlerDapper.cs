using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Queries;

public class GetLocationByIdHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetLocationByIdHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<Result<GetLocationDto, Errors>> Handle(GetLocationByIdRequest query, CancellationToken cancellationToken)
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
            map: (location, address, department) =>
            {
                if (locationDto is null)
                    locationDto = location;
                
                // address mapping
                locationDto.Address = address;
                
                // department mapping
                locationDto.Departments.Add(department);

                return locationDto;
            });
        
        if (locationDto is null)
        {
            return GeneralErrors.NotFound(query.LocationId, "Location").ToErrors();
        }
        
        return locationDto;
    }
}