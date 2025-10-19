using System.Collections.Generic;

namespace DirectoryService.Contracts.Departments;

public record GetDepartmentsTopDto(List<DepartmentsTopDto> Departments);