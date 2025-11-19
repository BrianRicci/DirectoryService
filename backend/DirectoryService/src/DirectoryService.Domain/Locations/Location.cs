using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Locations;

public class Location : ISoftDeletable
{
    public LocationId Id { get; private set; }

    public LocationName Name { get; private set; }
    
    public LocationAddress Address { get; private set; }

    public LocationTimezone Timezone { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; } 

    public DateTime? DeletedAt { get; private set; }
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departments;
    
    private readonly List<DepartmentLocation> _departments = [];
    
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
        LocationId id,
        LocationName name,
        LocationAddress address,
        LocationTimezone timezone)
    {
        return new Location(id, name, address, timezone);
    }
    
    public UnitResult<Error> Delete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        DeletedAt = DateTime.UtcNow;
        
        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> Restore()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        DeletedAt = null;
        
        return UnitResult.Success<Error>();
    }

    public Result Rename(LocationName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(this);
    }
}