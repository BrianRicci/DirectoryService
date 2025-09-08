using DirectoryService.Domain.Departments;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Positions;

public class Position
{
    // EF Core
    private Position()
    {
    }
    
    private Position(
        PositionId id,
        PositionName name,
        PositionDescription? description)
    {
        Id = id;
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
    
    private readonly List<DepartmentLocation> _departments = [];
    
    public PositionId Id { get; private set; }

    public PositionName Name { get; private set; }
    
    public PositionDescription? Description { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; } 
}