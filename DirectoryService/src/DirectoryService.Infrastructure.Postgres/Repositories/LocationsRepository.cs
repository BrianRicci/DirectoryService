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
    
    public async Task<UnitResult<Error>> SoftDeleteLocationsRelatedToDepartmentAsync(
        DepartmentId departmentId, CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"""
             WITH 
                 relation_location_ids AS (
                     SELECT l.location_id
                     FROM locations l
                              JOIN department_locations dl ON l.location_id = dl.location_id
                         AND dl.department_id = {departmentId.Value}
                              JOIN departments d ON dl.department_id = d.department_id
                     WHERE d.is_active = true
                 ),
                 
                 locations_count AS (
                     SELECT dl.location_id, COUNT(dl.location_id) AS count
                     FROM department_locations dl
                              JOIN relation_location_ids rl ON rl.location_id = dl.location_id
                     GROUP BY dl.location_id
                 ),
                 
                 locations_to_delete AS (
                     SELECT pc.location_id
                     FROM locations_count pc
                     WHERE pc.count = 1
                 )

             UPDATE locations l
             SET
                 is_active = false,
                 updated_at = {DateTime.UtcNow},
                 deleted_at = {DateTime.UtcNow}
             WHERE l.location_id in (SELECT * FROM locations_to_delete) 
             """, cancellationToken);

        return UnitResult.Success<Error>();
    }
}