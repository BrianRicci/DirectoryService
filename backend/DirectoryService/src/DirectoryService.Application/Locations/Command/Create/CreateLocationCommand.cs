using Core.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Command.Create;

public record CreateLocationCommand(CreateLocationRequest CreateLocationRequest) : ICommand;