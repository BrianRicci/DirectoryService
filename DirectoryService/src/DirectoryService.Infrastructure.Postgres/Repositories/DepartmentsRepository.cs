using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public DepartmentsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Errors>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return department.Id.Value;
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid().ToErrors();
        }
    }
    
    public async Task<Result<Department, Errors>> GetByIdAsync(DepartmentId departmentId, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .Include(d => d.DepartmentLocations)
            .FirstOrDefaultAsync(d => d.Id == departmentId && d.IsActive, cancellationToken);
        
        if (department is null)
            return GeneralErrors.NotFound(departmentId.Value).ToErrors();

        return department;
    }
    
    public async Task<bool> IsAllExistsAsync(List<DepartmentId> departmentIds, CancellationToken cancellationToken)
    {
        bool isAllExists = await _dbContext.Departments
            .Where(d => departmentIds.Contains(d.Id) && d.IsActive)
            .CountAsync(cancellationToken) == departmentIds.Count;
        
        return isAllExists;
    }
}