using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.Positions;

public class Position
{
    public PositionId Id { get; private set; }

    public PositionName Name { get; private set; }
    
    public PositionDescription? Description { get; private set; }

    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departments = [];
    
    private List<DepartmentPosition> _departments;
    
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
    
    public static Result<Position> Create(
        PositionName name,
        PositionDescription? description)
    {
        var id = new PositionId(Guid.NewGuid());
        
        return new Position(id, name, description);
    }
    
    public Result Rename(PositionName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(this);
    }
}