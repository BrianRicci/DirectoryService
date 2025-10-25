using System;

namespace DirectoryService.Contracts.Departments;

public record GetDepartmentChildsRequest(
    Guid ParentId,
    PaginationRequest? Pagination)
{
    public PaginationRequest Pagination { get; } = Pagination ?? new PaginationRequest();
}