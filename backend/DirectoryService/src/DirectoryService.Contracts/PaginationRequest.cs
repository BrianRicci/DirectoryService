namespace DirectoryService.Contracts;

public record PaginationRequest(int Page = 1, int PageSize = 20); // TODO: Потом избавиться от его использования в Departments