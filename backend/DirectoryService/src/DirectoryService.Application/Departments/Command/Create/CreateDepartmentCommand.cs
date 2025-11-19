using Core.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Command.Create;

public record CreateDepartmentCommand(CreateDepartmentRequest CreateDepartmentRequest) : ICommand;