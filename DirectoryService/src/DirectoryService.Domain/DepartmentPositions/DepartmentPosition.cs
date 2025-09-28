using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    public DepartmentPositionId Id { get; init; } = new DepartmentPositionId(Guid.NewGuid());
    
    public DepartmentId DepartmentId { get; init; } 

    public PositionId PositionId { get; init; }

    public DepartmentPosition(DepartmentId departmentId, PositionId positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }
    
    // EF Core
    private DepartmentPosition()
    {
    }
}