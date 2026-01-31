using Core.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.Command.UpdateDepartments;

public record UpdatePositionDepartmentsCommand(
    Guid Id, UpdatePositionDepartmentsRequest UpdatePositionDepartmentsRequest) : ICommand;