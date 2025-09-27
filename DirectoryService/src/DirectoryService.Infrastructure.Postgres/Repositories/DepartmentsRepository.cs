using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
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

    public async Task<Guid> AddAsync(Department department, CancellationToken cancellationToken)
    {
        await _dbContext.Departments.AddAsync(department, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return department.Id.Value;
    }
    
    public async Task<Result<Department, Errors>> GetByIdAsync(DepartmentId departmentId, CancellationToken cancellationToken)
    {
        var department = await _dbContext.Departments
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);
        
        if (department is null)
            return GeneralErrors.NotFound(departmentId.Value).ToErrors();

        return department;
    }
    
    public async Task<bool> IsAllExistsAsync(List<DepartmentId> departmentIds, CancellationToken cancellationToken)
    {
        bool isAllExists = await _dbContext.Departments
            .Where(l => departmentIds.Contains(l.Id) && l.IsActive)
            .CountAsync(cancellationToken) == departmentIds.Count;
        
        return isAllExists;
    }
}