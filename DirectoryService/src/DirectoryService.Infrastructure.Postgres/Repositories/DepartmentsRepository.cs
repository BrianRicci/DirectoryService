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
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .Include(d => d.DepartmentLocations)
            .FirstOrDefaultAsync(d => d.Id == departmentId && d.IsActive, cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(departmentId.Value);

        return department;
    }

    public async Task<Result<Department, Error>> GetByIdWithLock(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .FromSql(
                $"SELECT * FROM departments WHERE department_id = {departmentId.Value} AND is_active = true FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(departmentId.Value);

        return department;
    }

    public async Task<Result<List<Department>, Error>> GetDescendantsByPath(
        DepartmentPath path,
        CancellationToken cancellationToken)
    {
        var departments = await _dbContext.Departments
            .FromSqlInterpolated($"SELECT * FROM departments WHERE path <@ {path.Value}::ltree FOR UPDATE")
            .ToListAsync(cancellationToken);

        return departments;
    }

    public async Task<UnitResult<Error>> LockDescendants(DepartmentPath path, CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlAsync(
            $"SELECT * FROM departments WHERE path <@ {path.Value}::ltree FOR UPDATE", cancellationToken);

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