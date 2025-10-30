using System.Collections.Generic;

namespace DirectoryService.Contracts.Locations;

public record GetLocationsToDeleteDto(List<GetLocationToDeleteDto> Locations);