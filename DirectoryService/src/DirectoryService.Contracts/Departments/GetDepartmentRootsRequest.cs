using System;

namespace DirectoryService.Contracts.Departments;

public record GetDepartmentRootsRequest(
    PaginationRequest? Pagination,
    int Prefetch = 3)
{
    public PaginationRequest Pagination { get; } = Pagination ?? new PaginationRequest();
}
    
public record PaginationRequest(int Page = 1, int PageSize = 20);