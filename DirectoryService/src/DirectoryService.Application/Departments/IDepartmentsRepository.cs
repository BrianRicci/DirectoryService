using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Guid> AddAsync(Department department, CancellationToken cancellationToken);
    
    Task<Result<Department, Errors>> GetByIdAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<bool> IsAllExistsAsync(List<DepartmentId> departmentIds, CancellationToken cancellationToken);
}