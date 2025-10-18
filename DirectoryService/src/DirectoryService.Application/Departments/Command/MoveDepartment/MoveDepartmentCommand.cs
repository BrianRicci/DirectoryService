using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Command.MoveDepartment;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentRequest MoveDepartmentRequest) : ICommand;