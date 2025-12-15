namespace FileService.Contracts.GetMediaAssetsInfo;

public record GetMediaAssetsInfoResponse(IEnumerable<GetMediaAssetInfoForBatch> MediaAssets);

public record GetMediaAssetInfoForBatch(Guid Id, string Status, string? DownloadUrl);