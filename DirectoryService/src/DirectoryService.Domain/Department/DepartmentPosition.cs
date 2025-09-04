namespace DirectoryService.Domain.Department;

public class DepartmentPosition
{
    public Guid Id { get; init; }

    public Guid DepartmentId { get; init; } 

    public Guid LocationId { get; init; }
}