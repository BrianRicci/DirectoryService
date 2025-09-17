namespace DirectoryService.Contracts;

public record CreateLocationDto(
    string Name,
    AddressDto Address,
    string Timezone);