using System.Collections.Generic;

namespace DirectoryService.Contracts.Departments;

public record GetDepartmentNamesDto(List<DepartmentNamesDto> DepartmentNames);