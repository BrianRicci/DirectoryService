﻿using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.Command.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest CreateLocationRequest) : ICommand;