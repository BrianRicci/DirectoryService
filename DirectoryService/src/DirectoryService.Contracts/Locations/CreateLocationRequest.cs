namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(
    string Name,
    LocationAddressDto LocationAddress,
    string Timezone);