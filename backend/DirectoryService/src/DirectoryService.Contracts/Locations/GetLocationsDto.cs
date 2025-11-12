using System.Collections.Generic;

namespace DirectoryService.Contracts.Locations;

public record GetLocationsDto(List<GetLocationDto> Locations, long TotalCount);