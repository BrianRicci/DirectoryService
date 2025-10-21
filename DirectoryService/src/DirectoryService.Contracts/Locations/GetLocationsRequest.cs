using System;
using System.Collections.Generic;

namespace DirectoryService.Contracts.Locations;

public record GetLocationsRequest(
    string? Search,
    List<Guid>? DepartmentIds,
    bool? IsActive,
    PaginationRequest? Pagination)
{
    public PaginationRequest Pagination { get; } = Pagination ?? new PaginationRequest();
}
    
public record PaginationRequest(int Page = 1, int PageSize = 20);