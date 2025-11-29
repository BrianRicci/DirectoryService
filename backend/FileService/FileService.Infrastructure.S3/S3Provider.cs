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

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys)
    {
        try
        {
            IEnumerable<Task<string>> tasks = keys
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
                
                        return url;
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });
        
            string[] results = await Task.WhenAll(tasks);
        
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken)
    {
        var request = new InitiateMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            ContentType = contentType,
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

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Task<string>> tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        Verb = HttpVerb.PUT,
                        UploadId = uploadId,
                        PartNumber = 1,
                        Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };
                    
                    try
                    {
                        string? url = await _s3Client.GetPreSignedURLAsync(request);
                
                        return url;
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });
        
            string[] results = await Task.WhenAll(tasks);
        
            return results;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }
    
    public async Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        var request = new CompleteMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = key,
            UploadId = uploadId,
            PartETags = partETags.Select(p => new PartETag
            {
                ETag = p.ETag,
                PartNumber = p.PartNumber,
            }).ToList(),
        };
        
        try
        {
            var response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }
}