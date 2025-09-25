using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.Departments;

public class DepartmentLocation
{
    public DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        Id = new DepartmentLocationId(Guid.NewGuid());
        DepartmentId = departmentId;
        LocationId = locationId;
    }
    
    // EF Core
    private DepartmentLocation()
    {
    }
    
    public DepartmentLocationId Id { get; init; }
    
    public DepartmentId DepartmentId { get; init; } 
    
    public LocationId LocationId { get; init; }
}