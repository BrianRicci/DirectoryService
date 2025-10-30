﻿using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.Create;

public record CreatePositionCommand(CreatePositionRequest CreatePositionRequest) : ICommand;