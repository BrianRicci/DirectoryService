namespace FileService.Contracts.GetChunkUpload;

public record GetChunkUploadRequest(Guid MediaAssetId, ChunkUploadUrl ChunkUploadUrl);