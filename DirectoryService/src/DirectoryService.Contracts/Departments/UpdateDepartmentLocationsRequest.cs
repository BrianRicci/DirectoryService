using System;
using System.Collections.Generic;

namespace DirectoryService.Contracts.Departments;

public record UpdateDepartmentLocationsRequest(List<Guid> LocationIds);