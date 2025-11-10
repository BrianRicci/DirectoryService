using System.Collections.Generic;

namespace DirectoryService.Contracts.Departments;

public record GetDepartmentRootsDto(List<DepartmentRootsDto> Departments);