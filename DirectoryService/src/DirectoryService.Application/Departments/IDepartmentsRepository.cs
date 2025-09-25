using DirectoryService.Domain.Departments;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Guid> AddAsync(Department department, CancellationToken cancellationToken);
    
    Task<Department> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}