﻿using System;
using System.Collections.Generic;

namespace DirectoryService.Contracts.Departments;

public record CreateDepartmentRequest(
    string Name,
    string Identifier,
    Guid? ParentId,
    List<Guid> LocationIds);