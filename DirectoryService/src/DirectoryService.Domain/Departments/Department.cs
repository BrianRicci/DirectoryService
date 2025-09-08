using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Departments;

public class Department
{
    // EF Core
    private Department()
    {
    }
    
    private Department(
        DepartmentId id,
        DepartmentName name,
        DepartmentIdentifier identifier,
        Guid? parentId,
        List<DepartmentLocation> departmentLocations)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        UpdatedAt = DateTime.UtcNow;
        _locations = departmentLocations;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
    
    private readonly List<DepartmentLocation> _locations = [];
    private readonly List<DepartmentPosition> _positions = [];
    
    public DepartmentId Id { get; private set; }
    
    public DepartmentName Name { get; private set; }
    
    public DepartmentIdentifier Identifier { get; private set; }
    
    public Guid? ParentId { get; private set; }
    
    public DepartmentPath Path { get; private set; } 
    
    public short Depth { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
}