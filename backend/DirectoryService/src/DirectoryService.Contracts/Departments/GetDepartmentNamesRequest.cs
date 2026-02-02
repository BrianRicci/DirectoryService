namespace DirectoryService.Contracts.Departments;

public record GetDepartmentNamesRequest(
    string? Search,
    int Page = 1,
    int PageSize = 20);