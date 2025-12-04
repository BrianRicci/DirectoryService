namespace FileService.Contracts.AbortMultipartUpload;

public record AbortMultipartUploadRequest(Guid MediaAssetId, string UploadId);