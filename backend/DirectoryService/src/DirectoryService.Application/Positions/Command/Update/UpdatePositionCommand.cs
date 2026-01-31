using Core.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.Command.Update;

public record UpdatePositionCommand(Guid Id, UpdatePositionRequest UpdatePositionRequest) : ICommand;