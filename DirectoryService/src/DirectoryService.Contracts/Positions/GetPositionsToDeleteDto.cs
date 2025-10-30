using System.Collections.Generic;

namespace DirectoryService.Contracts.Positions;

public record GetPositionsToDeleteDto(List<GetPositionToDeleteDto> Positions);