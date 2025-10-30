namespace DirectoryService.Contracts.Positions;

public record GetPositionToDeleteDto : GetPositionDto
{
    public long Count { get; set; }
}