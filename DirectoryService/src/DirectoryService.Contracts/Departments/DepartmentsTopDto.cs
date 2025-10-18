namespace DirectoryService.Contracts.Departments;

public record DepartmentsTopDto : DepartmentDto
{
    public long PositionsCount { get; init; }
}