using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.DepartmentLocations;

public class DepartmentLocation
{
    public DepartmentLocationId Id { get; init; } = new DepartmentLocationId(Guid.Empty);
    
    public DepartmentId DepartmentId { get; init; } 
    
    public LocationId LocationId { get; init; }
    
    public DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
    
    // EF Core
    private DepartmentLocation()
    {
    }
}