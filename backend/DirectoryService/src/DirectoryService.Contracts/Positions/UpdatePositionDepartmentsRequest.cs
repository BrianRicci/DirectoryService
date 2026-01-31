using System;
using System.Collections.Generic;

namespace DirectoryService.Contracts.Positions;

public record UpdatePositionDepartmentsRequest(List<Guid> DepartmentIds);