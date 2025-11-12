using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<List<Department>, Error>> GetByIdsAsync(
        List<DepartmentId?> departmentIds, CancellationToken cancellationToken);
    
    Task<Result<Department, Error>> GetByIdWithLock(DepartmentId departmentId, CancellationToken cancellationToken);
    
    Task<Result<List<Department>, Error>> GetByParentIdsAsync(List<DepartmentId> parentIds, CancellationToken cancellationToken);

    Task<Result<List<Department>, Error>> GetDescendantsByPath(
        DepartmentPath path, CancellationToken cancellationToken);

    Task<Result<List<Department>, Error>> GetInactiveAsync(
        FilterOptions timeFilterOptions, CancellationToken cancellationToken);
    
    Task<UnitResult<Error>> LockDescendants(DepartmentPath oldPath, CancellationToken cancellationToken);

    Task<UnitResult<Error>> BulkDeleteAsync(List<DepartmentId> departmentIds, CancellationToken cancellationToken);

    Task<UnitResult<Error>> BulkUpdateDescendantsPath(
        DepartmentPath oldPath,
        DepartmentPath newPath,
        int depthDelta,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);

    Task<bool> IsAllExistsAsync(List<DepartmentId> departmentIds, CancellationToken cancellationToken);
}