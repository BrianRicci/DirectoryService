using System;

namespace DirectoryService.Contracts.Departments;

public record MoveDepartmentRequest(Guid? ParentId);