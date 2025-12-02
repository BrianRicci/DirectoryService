namespace FileService.Contracts.StartMultipartUpload;

public record StartMultipartUploadRequest(
    string FileName,
    string ContentType,
    long FileSize,
    string AssetType,
    string Context,
    Guid ContextId);