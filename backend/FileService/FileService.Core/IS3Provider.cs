using Amazon.S3.Model;
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

    Task<Result<IReadOnlyDictionary<StorageKey, string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys);

    Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey key,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<ChunkUploadUrl, Error>> GenerateChunkUploadUrlAsync(
        StorageKey key,
        ChunkUploadUrl chunkUploadUrl,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<CompleteMultipartUploadResponse, Error>> CompleteMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        List<PartETagDto> partETags,
        CancellationToken cancellationToken);

    Task<UnitResult<Error>> AbortMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        CancellationToken cancellationToken);

    Task<Result<ListMultipartUploadsResponse, Error>> ListMultipartUploadsAsync(
        string bucketName,
        CancellationToken cancellationToken);
}