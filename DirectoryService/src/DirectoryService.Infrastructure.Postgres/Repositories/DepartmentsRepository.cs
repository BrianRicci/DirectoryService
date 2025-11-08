using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DepartmentsRepository> _logger;

    public DepartmentsRepository(DirectoryServiceDbContext dbContext, ILogger<DepartmentsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return department.Id.Value;
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid();
        }
    }

    public async Task<Result<Department, Error>> GetByIdAsync(
        DepartmentId departmentId, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .Include(d => d.DepartmentLocations)
            .FirstOrDefaultAsync(d => d.Id == departmentId && d.IsActive, cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(departmentId.Value);

        return department;
    }

    public async Task<Result<List<Department>, Error>> GetByIdsAsync(
        List<DepartmentId?> departmentIds, CancellationToken cancellationToken)
    {
        var departments = await _dbContext.Departments
            .Where(d => departmentIds.Contains(d.Id) && d.IsActive)
            .ToListAsync(cancellationToken);

        return departments;
    }

    public async Task<Result<Department, Error>> GetByIdWithLock(
        DepartmentId departmentId, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .FromSql(
                $"SELECT * FROM departments WHERE department_id = {departmentId.Value} AND is_active = true FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(departmentId.Value);

        return department;
    }
    
    public async Task<Result<List<Department>, Error>> GetByParentIdsAsync(
        List<DepartmentId> parentIds, CancellationToken cancellationToken)
    {
        var departments = await _dbContext.Departments
            .Where(d => d.ParentId != null && parentIds.Contains(d.ParentId))
            .ToListAsync(cancellationToken);

        return departments;
    }

    public async Task<Result<List<Department>, Error>> GetDescendantsByPath(
        DepartmentPath path, CancellationToken cancellationToken)
    {
        var departments = await _dbContext.Departments
            .FromSqlInterpolated($"SELECT * FROM departments WHERE path <@ {path.Value}::ltree FOR UPDATE")
            .ToListAsync(cancellationToken);

        return departments;
    }
    
    public async Task<Result<List<Department>, Error>> GetInactiveAsync(
        FilterOptions timeFilterOptions, CancellationToken cancellationToken)
    {
        var allDepartments = await _dbContext.Departments.ToListAsync(cancellationToken); 
        var departments = await _dbContext.Departments
            .Where(d => !d.IsActive && d.DeletedAt < timeFilterOptions.ThresholdDate)
            .ToListAsync(cancellationToken);

        return departments;
    }

    public async Task<UnitResult<Error>> LockDescendants(DepartmentPath path, CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"SELECT * FROM departments WHERE path <@ {path.Value}::ltree FOR UPDATE", cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> BulkDeleteAsync(
        List<DepartmentId> departmentIds, CancellationToken cancellationToken)
    {
        var departmentIdsArray = departmentIds.Select(d => d.Value).ToArray();

        // удаление уже выбранных департаментов и локаций и позиций, которые остались после каскадного удаления связей
        await _dbContext.Database.ExecuteSqlAsync(
            $"""
             DELETE FROM departments
             WHERE department_id = ANY({departmentIdsArray})
             RETURNING department_id
             """, cancellationToken);
        
        await _dbContext.Database.ExecuteSqlAsync(
            $"""
             WITH 
                 delete_locations AS (
                    DELETE FROM locations
                    WHERE is_active = false
                           AND NOT EXISTS (
                               SELECT 1
                               FROM department_locations
                               WHERE location_id = locations.location_id
                           )
                    RETURNING location_id
                 ),
                 
                 delete_positions AS (
                    DELETE FROM positions
                    WHERE is_active = false
                        AND NOT EXISTS (
                            SELECT 1
                            FROM department_positions
                            WHERE position_id = positions.position_id
                        )
                    RETURNING position_id
                 )
             
             SELECT
                 (SELECT COUNT(*) FROM delete_locations) AS deleted_locations,
                 (SELECT COUNT(*) FROM delete_positions) AS deleted_positions
             """, cancellationToken);
        
        return UnitResult.Success<Error>();
    }
    
    public async Task<UnitResult<Error>> BulkUpdateDescendantsPath(
        DepartmentPath oldPath,
        DepartmentPath newPath,
        int depthDelta,
        CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"""
             UPDATE departments 
             SET path = {newPath.Value}::ltree || subpath(path, nlevel({oldPath.Value}::ltree)),
                 depth = depth + {depthDelta},
                 updated_at = {DateTime.UtcNow}
             WHERE path <@ {oldPath.Value}::ltree AND path != {oldPath.Value}::ltree
             """, cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save changes");
            return GeneralErrors.Failure("Failed to save changes");
        }
    }

    public async Task<bool> IsAllExistsAsync(List<DepartmentId> departmentIds, CancellationToken cancellationToken)
    {
        bool isAllExists = await _dbContext.Departments
            .Where(d => departmentIds.Contains(d.Id) && d.IsActive)
            .CountAsync(cancellationToken) == departmentIds.Count;

        return isAllExists;
    }
}