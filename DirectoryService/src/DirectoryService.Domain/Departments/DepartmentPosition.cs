using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Departments;

public class DepartmentPosition
{
    // EF Core
    private DepartmentPosition()
    {
    }
    
    public DepartmentPositionId Id { get; init; }

    public Guid DepartmentId { get; init; } 

    public Guid PositionId { get; init; }
}