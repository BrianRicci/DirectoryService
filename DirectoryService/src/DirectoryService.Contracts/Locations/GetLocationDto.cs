using System;

namespace DirectoryService.Contracts.Locations;

public record GetLocationDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;
    
    public LocationAddressDto Address { get; init; } = new LocationAddressDto(
        string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public string Timezone { get; init; } = string.Empty;

    public bool IsActive { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime UpdatedAt { get; init; } 
}