using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Departments;

public class DepartmentPosition
{
    // EF Core
    private DepartmentPosition()
    {
    }
    
    public DepartmentPositionId Id { get; init; }

    public DepartmentId DepartmentId { get; init; } 

    public PositionId PositionId { get; init; }
}