using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Departments;

public class DepartmentLocation
{
    // EF Core
    private DepartmentLocation()
    {
    }
    
    public DepartmentLocationId Id { get; init; }
    
    public Guid DepartmentId { get; init; } 
    
    public Guid LocationId { get; init; }
}