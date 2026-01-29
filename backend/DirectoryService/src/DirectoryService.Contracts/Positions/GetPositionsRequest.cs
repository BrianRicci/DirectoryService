using System;
using System.Collections.Generic;

namespace DirectoryService.Contracts.Positions;

public record GetPositionsRequest(
    string? Search,
    List<Guid>? DepartmentIds,
    bool? IsActive,
    string? sortBy,
    string? sortOrder,
    int Page = 1,
    int PageSize = 20);