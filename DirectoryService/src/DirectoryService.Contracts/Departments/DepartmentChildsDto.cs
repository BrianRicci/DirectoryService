namespace DirectoryService.Contracts.Departments;

public record DepartmentChildsDto : DepartmentDto
{
    public bool HasMoreChildren { get; init; }
}