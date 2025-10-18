using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.Command.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest CreateDepartmentRequest) : ICommand;