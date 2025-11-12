namespace DirectoryService.Contracts.Locations;

public record GetLocationToDeleteDto : GetLocationDto
{
    public long Count { get; set; }
}