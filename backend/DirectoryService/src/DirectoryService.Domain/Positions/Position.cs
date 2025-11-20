using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Positions;

public class Position : ISoftDeletable
{
    public PositionId Id { get; private set; }

    public PositionName Name { get; private set; }
    
    public PositionDescription? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    public DateTime? DeletedAt { get; private set; }

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
    
    public Result Rename(PositionName name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(this);
    }
}