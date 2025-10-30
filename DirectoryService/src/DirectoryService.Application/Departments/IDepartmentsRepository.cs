using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Errors>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Errors>> GetByIdAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<Department, Errors>> GetByIdWithLock(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<List<Department>, Errors>> GetDescendantsByPath(
        DepartmentPath path,
        CancellationToken cancellationToken);
    
    Task<bool> IsAllExistsAsync(List<DepartmentId> departmentIds, CancellationToken cancellationToken);
    
    Task<UnitResult<Errors>> LockDescendants(DepartmentPath oldPath, CancellationToken cancellationToken);

    Task<UnitResult<Errors>> BulkUpdateDescendantsPath(
        DepartmentPath oldPath,
        DepartmentPath newPath,
        int depthDelta,
        CancellationToken cancellationToken);
    
    Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken);
}