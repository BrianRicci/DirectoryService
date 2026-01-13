namespace FileService.Contracts.Dtos.CompleteMultipartUpload;

public record CompleteMultipartUploadRequest(Guid MediaAssetId, string UploadId, List<PartETagDto> PartETags);