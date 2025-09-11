namespace DirectoryService.Contracts;

public record CreateLocationDto(
    string name,
    AddressDto address,
    string timezone);