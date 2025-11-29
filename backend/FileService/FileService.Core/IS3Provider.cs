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
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken);
}