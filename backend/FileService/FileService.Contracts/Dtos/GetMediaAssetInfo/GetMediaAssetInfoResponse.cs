namespace FileService.Contracts.Dtos.GetMediaAssetInfo;

public record GetMediaAssetInfoResponse(
    Guid Id,
    string Status,
    string AssetType,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    FileInfo FileInfo,
    string? DownloadUrl);

public record FileInfo(
    string FileName,
    string ContentType,
    long FileSize);