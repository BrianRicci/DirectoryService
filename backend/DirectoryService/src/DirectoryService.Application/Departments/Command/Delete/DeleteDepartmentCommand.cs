using Core.Abstractions;

namespace DirectoryService.Application.Departments.Command.Delete;

public record DeleteDepartmentCommand(Guid DepartmentId) : ICommand;