using CSharpFunctionalExtensions;
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
        UpdatedAt = DateTime.UtcNow;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
    
    public static Result<Location> Create(
        string name,
        string country,
        string region,
        string city,
        string street,
        string house,
        string timezone)
    {
        Result<LocationName> nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure<Location>(nameResult.Error);

        Result<LocationAddress> addressResult = LocationAddress.Create(country, region, city, street, house);
        if (addressResult.IsFailure)
            return Result.Failure<Location>(addressResult.Error);
        
        Result<LocationTimezone> timezoneResult = LocationTimezone.Create(timezone);
        if (timezoneResult.IsFailure)
            return Result.Failure<Location>(timezoneResult.Error);
        
        var id = new LocationId(Guid.NewGuid());
        
        return new Location(id, nameResult.Value, addressResult.Value, timezoneResult.Value);
    }

    public Result Rename(string name)
    {
        Result<LocationName> nameResult = LocationName.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure<Location>(nameResult.Error);
        
        Name = nameResult.Value;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(this);
    }
}