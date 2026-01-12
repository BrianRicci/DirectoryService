namespace FileService.Contracts.Dtos.AbortMultipartUpload;

public record AbortMultipartUploadRequest(Guid MediaAssetId, string UploadId);