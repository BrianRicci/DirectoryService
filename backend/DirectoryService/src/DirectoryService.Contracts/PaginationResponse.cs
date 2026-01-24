using System.Collections.Generic;

namespace DirectoryService.Contracts;

public record PaginationResponse<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    int Page,
    int PageSize,
    long TotalPages);