namespace FileService.Contracts.StartMultipartUpload;

public record StartMultipartUploadResponse(Guid MediaAssetId, string UploadId, IReadOnlyList<ChunkUploadUrl> ChunkUrls, long ChunkSize);