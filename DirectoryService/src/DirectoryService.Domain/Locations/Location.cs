using DirectoryService.Domain.Departments;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Locations;

public class Location
{
    // EF Core
    private Location()
    {
    }
    
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
    
    private readonly List<DepartmentLocation> _departments = [];
    
    public LocationId Id { get; private set; }

    public LocationName Name { get; private set; }
    
    public LocationAddress Address { get; private set; }

    public LocationTimezone Timezone { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; } 
}