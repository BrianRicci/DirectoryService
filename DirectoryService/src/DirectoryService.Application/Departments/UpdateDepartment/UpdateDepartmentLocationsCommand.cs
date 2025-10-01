﻿using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public record UpdateDepartmentLocationsCommand(Guid Id, UpdateDepartmentLocationsRequest UpdateDepartmentLocationsRequest) : ICommand;