using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Departments;

public class DepartmentLocation
{
    // EF Core
    private DepartmentLocation()
    {
    }
    
    public DepartmentLocationId Id { get; init; }
    
    public DepartmentId DepartmentId { get; init; } 
    
    public LocationId LocationId { get; init; }
}