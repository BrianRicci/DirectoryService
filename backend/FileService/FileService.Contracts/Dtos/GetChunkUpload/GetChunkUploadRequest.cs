namespace FileService.Contracts.Dtos.GetChunkUpload;

public record GetChunkUploadRequest(Guid MediaAssetId, string UploadId, ChunkUploadUrl ChunkUploadUrl);