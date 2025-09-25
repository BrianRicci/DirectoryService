namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(
    string Name,
    LocationAddressDto LocationAddress,
    string Timezone);