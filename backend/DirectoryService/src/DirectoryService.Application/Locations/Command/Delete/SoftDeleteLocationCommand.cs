using Core.Abstractions;

namespace DirectoryService.Application.Locations.Command.Delete;

public record SoftDeleteLocationCommand(Guid LocationId) : ICommand;