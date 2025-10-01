using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Department department, CancellationToken cancellationToken);
    
    Task<Result<Department, Errors>> GetByIdAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<bool> IsAllExistsAsync(List<DepartmentId> departmentIds, CancellationToken cancellationToken);
}