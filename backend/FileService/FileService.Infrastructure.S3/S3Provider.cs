using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel;

namespace FileService.Infrastructure.S3;

public class S3Provider : IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Provider> _logger;
    private readonly S3Options _s3Options;

    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> s3Options, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Options = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(_s3Options.MaxConcurrentRequests);
    }

    public async Task<UnitResult<Error>> UploadFileAsync(
        StorageKey key,
        Stream fileStream,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            InputStream = fileStream,
            ContentType = mediaData.ContentType.Value,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
        
        return UnitResult.Success<Error>();
    }

    public async Task<Result<string, Error>> DownloadFileAsync(
        StorageKey key,
        string tempPath,
        CancellationToken cancellationToken)
    {
        var response = await _s3Client.GetObjectAsync(key.Bucket, key.Key, cancellationToken);

        return response.BucketName;
    }

    public async Task<Result<string, Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(key.Bucket, key.Key, cancellationToken);
            
            return key.Key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file");
            
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateUploadUrlAsync(
        StorageKey key,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            ContentType = mediaData.ContentType.Value,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
        };
        
        try
        {
            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate upload url");
            
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
        };
        
        try
        {
            string? response = await _s3Client.GetPreSignedURLAsync(request);
            
            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate download url");
            
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyDictionary<StorageKey, string>, Error>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> keys)
    {
        try
        {
            IEnumerable<Task<(StorageKey, string)>> tasks = keys
                .Select(async key =>
                {
                    await _requestsSemaphore.WaitAsync();
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = key.Bucket,
                        Key = key.Key,
                        Verb = HttpVerb.GET,
                        Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };
                    
                    try
                    {
                        string? url = await _s3Client.GetPreSignedURLAsync(request);
                
                        return (key, url);
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });
        
            var results = await Task.WhenAll(tasks);
            
            return results.ToDictionary();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey key,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        var request = new InitiateMultipartUploadRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            ContentType = mediaData.ContentType.Value,
        };
        
        try
        {
            InitiateMultipartUploadResponse result = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);

            return result.UploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start multipart upload");
            
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<ChunkUploadUrl, Error>> GenerateChunkUploadUrlAsync(
        StorageKey key,
        string uploadId,
        int partNumber,
        CancellationToken cancellationToken)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            Verb = HttpVerb.PUT,
            UploadId = uploadId,
            PartNumber = partNumber,
            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
        };
        
        try
        {
            string? url = await _s3Client.GetPreSignedURLAsync(request);

            return new ChunkUploadUrl(partNumber, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate chunk upload url");
            
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Task<ChunkUploadUrl>> tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = key.Bucket,
                        Key = key.Key,
                        Verb = HttpVerb.PUT,
                        UploadId = uploadId,
                        PartNumber = partNumber,
                        Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };
                    
                    try
                    {
                        string? url = await _s3Client.GetPreSignedURLAsync(request);
                
                        return new ChunkUploadUrl(partNumber, url);
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });
        
            ChunkUploadUrl[] results = await Task.WhenAll(tasks);
        
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate all chunks upload urls");
            
            return S3ErrorMapper.ToError(ex);
        }
    }
    
    public async Task<Result<CompleteMultipartUploadResponse, Error>> CompleteMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        List<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        var request = new CompleteMultipartUploadRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            UploadId = uploadId,
            PartETags = partETags.Select(p => new PartETag
            {
                ETag = p.ETag,
                PartNumber = p.PartNumber,
            }).ToList(),
        };
        
        try
        {
            CompleteMultipartUploadResponse response = await _s3Client.CompleteMultipartUploadAsync(
                request, cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete multipart upload");
            
            return S3ErrorMapper.ToError(ex);
        }
    }
    
    public async Task<UnitResult<Error>> AbortMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        CancellationToken cancellationToken)
    {
        var request = new AbortMultipartUploadRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            UploadId = uploadId,
        };
        
        try
        {
            await _s3Client.AbortMultipartUploadAsync(request, cancellationToken);
            
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to abort multipart upload");
            
            return S3ErrorMapper.ToError(ex);
        }
    }
    
    public async Task<Result<ListMultipartUploadsResponse, Error>> ListMultipartUploadsAsync(
        string bucketName,
        CancellationToken cancellationToken)
    {
        var request = new ListMultipartUploadsRequest
        {
            BucketName = bucketName,
        };
        
        try
        {
            var response = await _s3Client.ListMultipartUploadsAsync(request, cancellationToken);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list multipart uploads");
            
            return S3ErrorMapper.ToError(ex);
        }
    }
}