namespace FileService.Contracts.CompleteMultipartUpload;

public record CompleteMultipartUploadRequest(Guid MediaAssetId, string UploadId, List<PartETagDto> PartETags);