using Core.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Command.Update;

public record UpdateDepartmentLocationsCommand(Guid Id, UpdateDepartmentLocationsRequest UpdateDepartmentLocationsRequest) : ICommand;