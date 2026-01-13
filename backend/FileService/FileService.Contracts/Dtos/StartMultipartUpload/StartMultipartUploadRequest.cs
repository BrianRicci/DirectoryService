namespace FileService.Contracts.Dtos.StartMultipartUpload;

public record StartMultipartUploadRequest(
    string FileName,
    string ContentType,
    long FileSize,
    string AssetType);