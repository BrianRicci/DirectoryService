using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.Locations;

public class Location
{
    public LocationId Id { get; private set; }

    public LocationName Name { get; private set; }
    
    public LocationAddress Address { get; private set; }

    public LocationTimezone Timezone { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; } 
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departments = [];
    
    private List<DepartmentLocation> _departments;
    
    // EF Core
    private Location()
    {
    }
    
    private Location(
        LocationId id,
        LocationName name,
        LocationAddress address,
        LocationTimezone timezone)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
    
    public static Result<Location> Create(
        LocationName name,
        LocationAddress address,
        LocationTimezone timezone)
    {
        var id = new LocationId(Guid.NewGuid());
        
        return new Location(id, name, address, timezone);
    }

    public Result Rename(LocationName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(this);
    }
}