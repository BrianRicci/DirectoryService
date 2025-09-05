using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Departments;

public class Department
{
    private Department(
        DepartmentName departmentName,
        DepartmentIdentifier departmentIdentifier,
        Guid? parentId,
        List<DepartmentLocation> departmentLocations)
    {
        Id = Guid.NewGuid();
        DepartmentName = departmentName;
        DepartmentIdentifier = departmentIdentifier;
        ParentId = parentId;
        UpdatedAt = DateTime.UtcNow;
        _locations = departmentLocations;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
    
    private readonly List<DepartmentLocation> _locations;
    private readonly List<DepartmentPosition> _positions = [];
    
    public Guid Id { get; private set; }
    
    public DepartmentName DepartmentName { get; private set; }
    
    public DepartmentIdentifier DepartmentIdentifier { get; private set; }
    
    public Guid? ParentId { get; private set; }
    
    public DepartmentPath DepartmentPath { get; private set; } 
    
    public short Depth { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
}