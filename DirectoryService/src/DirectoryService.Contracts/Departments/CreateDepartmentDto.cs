using System;
using System.Collections;
using System.Collections.Generic;

namespace DirectoryService.Contracts.Departments;

public record CreateDepartmentDto(
    string Name,
    string Identifier,
    Guid? ParentId,
    List<Guid> LocationIds);