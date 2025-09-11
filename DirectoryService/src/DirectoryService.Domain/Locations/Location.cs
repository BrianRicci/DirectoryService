using DirectoryService.Domain.Departments;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Locations;

public class Location
{
    public Location(
        LocationId id,
        LocationName name,
        LocationAddress address,
        LocationTimezone timezone)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        UpdatedAt = DateTime.UtcNow;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
    
    // EF Core
    private Location()
    {
    }
    
    public LocationId Id { get; private set; }

    public LocationName Name { get; private set; }
    
    public LocationAddress Address { get; private set; }

    public LocationTimezone Timezone { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; } 
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departments = [];
    
    private List<DepartmentLocation>? _departments;
}