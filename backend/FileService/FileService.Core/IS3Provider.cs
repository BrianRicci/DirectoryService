using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Domain;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IS3Provider
{
    Task<UnitResult<Error>> UploadFileAsync(
        StorageKey key,
        Stream fileStream,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> DownloadFileAsync(
        StorageKey key,
        string tempPath,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateUploadUrlAsync(
        StorageKey key,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey key);

    Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys);

    Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey key,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateChunkUploadUrlAsync(
        StorageKey key,
        string uploadId,
        CancellationToken cancellationToken);
    
    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        List<PartETagDto> partETags,
        CancellationToken cancellationToken);
    
    Task<Result<string, Error>> AbortMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        CancellationToken cancellationToken);
    
    Task<Result<IReadOnlyList<string>, Error>> ListMultipartUploadsAsync(
        string bucketName,
        CancellationToken cancellationToken);
}