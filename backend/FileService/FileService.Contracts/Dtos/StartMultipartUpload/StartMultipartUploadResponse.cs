namespace FileService.Contracts.Dtos.StartMultipartUpload;

public record StartMultipartUploadResponse(Guid MediaAssetId, string UploadId, IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls, int ChunkSize);