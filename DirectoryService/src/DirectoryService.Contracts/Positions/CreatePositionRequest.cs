using System;
using System.Collections.Generic;

namespace DirectoryService.Contracts.Positions;

public record CreatePositionRequest(string Name, string? Description, List<Guid> DepartmentIds);