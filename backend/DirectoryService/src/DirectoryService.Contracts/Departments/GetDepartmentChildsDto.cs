using System.Collections.Generic;

namespace DirectoryService.Contracts.Departments;

public record GetDepartmentChildsDto(List<DepartmentChildsDto> Departments);