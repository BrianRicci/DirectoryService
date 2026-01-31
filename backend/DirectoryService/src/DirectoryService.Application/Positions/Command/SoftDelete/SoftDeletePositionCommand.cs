using Core.Abstractions;

namespace DirectoryService.Application.Positions.Command.SoftDelete;

public record SoftDeletePositionCommand(Guid PositionId) : ICommand;