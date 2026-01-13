namespace FileService.Contracts.Dtos.GetMediaAssetsInfo;

public record GetMediaAssetsInfoResponse(IEnumerable<GetMediaAssetInfoForBatch> MediaAssets);

public record GetMediaAssetInfoForBatch(Guid Id, string Status, string? DownloadUrl);