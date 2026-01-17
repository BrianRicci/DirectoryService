using Core.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Command.Update;

public record UpdateLocationCommand(Guid Id, UpdateLocationRequest UpdateLocationRequest) : ICommand;