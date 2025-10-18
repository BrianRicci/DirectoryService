using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Command.UpdateDepartment;

public record UpdateDepartmentLocationsCommand(Guid Id, UpdateDepartmentLocationsRequest UpdateDepartmentLocationsRequest) : ICommand;