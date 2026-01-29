using Core.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.Command.Create;

public record CreatePositionCommand(CreatePositionRequest CreatePositionRequest) : ICommand;