using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly IDbConnectionFactory _connectionFactory;

    public LocationsRepository(
        DirectoryServiceDbContext dbContext,
        IDbConnectionFactory connectionFactory)
    {
        _dbContext = dbContext;
        _connectionFactory = connectionFactory;
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
            .Where(l => locationIds.Contains(l.Id) && l.IsActive)
            .CountAsync(cancellationToken) == locationIds.Count;
        
        return isAllExists;
    }
    
    // метод возвращает null если не найдена локация или найдено несколько локаций связанных с 1 активным департаментом
    // сделано для того, чтобы не пришлось вытаскивать весь список локаций в хэндлер, которые по итогу не будут использоваться
    public async Task<Result<GetLocationsToDeleteDto, Error>> GetLocationsRelatedToDepartmentAsync(
        DepartmentId departmentId, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var testLocations = await connection.QueryAsync<GetLocationDto>(
            "SELECT * FROM locations");
            
        var locationDtos =
            await connection.QueryAsync<GetLocationToDeleteDto, LocationAddressDto, long, GetLocationToDeleteDto>(
            """
            WITH related_location_ids AS (
                SELECT l.location_id
                FROM locations l
                    JOIN department_locations dl ON l.location_id = dl.location_id 
                        AND dl.department_id = @departmentId
            )

            SELECT l.location_id,
                   l.name, 
                   l.is_active,
                   l.timezone,
                   l.created_at,
                   l.updated_at,
                   l.deleted_at,
                   l.country,
                   l.region,
                   l.city,
                   l.street,
                   l.house,
                   COUNT(l.location_id) AS count
            FROM locations l
               JOIN department_locations dl ON l.location_id = dl.location_id
               JOIN departments d ON dl.department_id = d.department_id
            WHERE dl.location_id IN (SELECT location_id FROM related_location_ids)
            AND d.is_active = true
            GROUP BY l.location_id;
            """,
            param: new { departmentId = departmentId.Value, },
            splitOn: "country, count",
            map: (location, address, count) =>
            {
                // address mapping
                location.Address = address;
                
                // count mapping
                location.Count = count;
                
                return location;
            });

        var result = locationDtos.Where(dto => dto.Count == 1).ToList();
        
        return new GetLocationsToDeleteDto(result);
    }
}