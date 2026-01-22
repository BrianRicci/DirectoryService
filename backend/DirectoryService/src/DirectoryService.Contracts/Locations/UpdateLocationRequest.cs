namespace DirectoryService.Contracts.Locations;

public record UpdateLocationRequest(string Name, LocationAddressDto LocationAddress, string Timezone);