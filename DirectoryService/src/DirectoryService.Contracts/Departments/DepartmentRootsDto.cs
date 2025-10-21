namespace DirectoryService.Contracts.Departments;

public record DepartmentRootsDto : DepartmentDto
{
    public bool HasMoreChildren { get; set; }
}