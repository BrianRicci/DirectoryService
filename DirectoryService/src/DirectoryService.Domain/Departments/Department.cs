using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using Shared;

namespace DirectoryService.Domain.Departments;

public class Department
{
    public DepartmentId Id { get; private set; }
    
    public DepartmentName Name { get; private set; }
    
    public DepartmentIdentifier Identifier { get; private set; }
    
    public DepartmentId? ParentId { get; private set; }
    
    public DepartmentPath Path { get; private set; } 
    
    public short Depth { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _locations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _positions;
    
    private readonly List<DepartmentLocation> _locations = [];
    
    private readonly List<DepartmentPosition> _positions = [];
    
    // EF Core
    private Department()
    {
    }
    
    private Department(
        DepartmentId id,
        DepartmentId? parentId,
        DepartmentName name,
        DepartmentIdentifier identifier,
        DepartmentPath path,
        short depth)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
        Identifier = identifier;
        Path = path;
        Depth = depth;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

    public static Result<Department> CreateParent(
        DepartmentName name,
        DepartmentIdentifier identifier,
        DepartmentPath path,
        short depth,
        DepartmentId? id = null)
    {
        return new Department(
            id ?? new DepartmentId(Guid.NewGuid()),
            null,
            name,
            identifier,
            path,
            depth);
    }

    public static Result<Department> CreateChild(
        DepartmentId parentId,
        DepartmentName name,
        DepartmentIdentifier identifier,
        DepartmentPath path,
        short depth,
        DepartmentId? id = null)
    {
        return new Department(
            id ?? new DepartmentId(Guid.NewGuid()),
            parentId,
            name,
            identifier,
            path,
            depth);
    }
    
    public Result Rename(DepartmentName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(this);
    }
    
    public UnitResult<Error> AddDepartmentLocations(List<DepartmentLocation> locations)
    {
        _locations.AddRange(locations);
        UpdatedAt = DateTime.UtcNow;
        
        return UnitResult.Success<Error>();
    }
}