using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using Shared;

namespace DirectoryService.Domain.Positions;

public class Position
{
    public PositionId Id { get; private set; }

    public PositionName Name { get; private set; }
    
    public PositionDescription? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departments;
    
    private readonly List<DepartmentPosition> _departments = [];
    
    // EF Core
    private Position()
    {
    }

    private Position(
        PositionId id,
        PositionName name,
        PositionDescription? description,
        List<DepartmentPosition> departmentPositions)
    {
        Id = id;
        Name = name;
        Description = description;
        _departments = departmentPositions;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

    public static Result<Position> Create(
        PositionId id,
        PositionName name,
        PositionDescription? description,
        List<DepartmentPosition> departmentPositions)
    {
        return new Position(id, name, description, departmentPositions);
    }

    public Result Rename(PositionName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(this);
    }
}