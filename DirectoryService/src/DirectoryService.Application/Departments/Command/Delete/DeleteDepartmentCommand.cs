using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.Command.Delete;

public record DeleteDepartmentCommand(Guid DepartmentId) : ICommand;