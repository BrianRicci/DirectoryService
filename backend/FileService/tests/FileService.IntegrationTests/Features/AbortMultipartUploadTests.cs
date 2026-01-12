using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dtos.StartMultipartUpload;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;
using AbortMultipartUploadRequest = FileService.Contracts.Dtos.AbortMultipartUpload.AbortMultipartUploadRequest;
using CompleteMultipartUploadRequest = FileService.Contracts.Dtos.CompleteMultipartUpload.CompleteMultipartUploadRequest;

namespace FileService.IntegrationTests.Features;

public class AbortMultipartUploadTests : FileServiceTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;

    public AbortMultipartUploadTests(IntegrationTestsWebFactory factory) 
        : base(factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task AbortMultipartUpload_AbortsMultipartUpload()
    {
        // arrange 
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));
        
        StartMultipartUploadResponse startMultipartUploadResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        var request = new AbortMultipartUploadRequest(startMultipartUploadResponse.MediaAssetId, startMultipartUploadResponse.UploadId);
        
        // act
        HttpResponseMessage abortMultipartUploadResponse = await AppHttpClient.PostAsJsonAsync("api/files/multipart/abort", request, cancellationToken);

        Result<AbortMultipartUploadResponse, Errors> abortMultipartUploadResult =
            await abortMultipartUploadResponse.HandleResponseAsync<AbortMultipartUploadResponse>(cancellationToken);
        
        // assert
        Assert.True(abortMultipartUploadResult.IsSuccess);

        await ExecuteInDb(async db =>
        {
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(ma => ma.Id == startMultipartUploadResponse.MediaAssetId, cancellationToken);

            Assert.Equal(MediaStatus.FAILED, mediaAsset?.Status);
            Assert.NotNull(mediaAsset);

            IAmazonS3 amazonS3Client = _factory.Services.GetRequiredService<IAmazonS3>();

            ListObjectsV2Request listObjectsV2Request = new ListObjectsV2Request
            {
                BucketName = mediaAsset.RawKey.Bucket, Prefix = mediaAsset.RawKey.Value, MaxKeys = 1,
            };

            ListObjectsV2Response listObjectsV2Response = await amazonS3Client.ListObjectsV2Async(listObjectsV2Request, cancellationToken);

            Assert.Null(listObjectsV2Response.S3Objects);
        });
    }

    private async Task<StartMultipartUploadResponse> StartMultipartUpload(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        // arrange
        var request = new StartMultipartUploadRequest(fileInfo.Name, "video/mp4", fileInfo.Length, "video");
        
        // act
        HttpResponseMessage startMultipartResponse = await AppHttpClient.PostAsJsonAsync("api/files/multipart/start", request, cancellationToken);

        Result<StartMultipartUploadResponse, Errors> startMultipartResult =
            await startMultipartResponse.HandleResponseAsync<StartMultipartUploadResponse>(cancellationToken);
        
        // assert
        Assert.True(startMultipartResult.IsSuccess);
        Assert.NotNull(startMultipartResult.Value.UploadId);
        
        await ExecuteInDb(async db =>
        {
            var mediaAssets = await db.MediaAssets.ToListAsync(cancellationToken);
            
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(ma => ma.Id == startMultipartResult.Value.MediaAssetId, cancellationToken);
            
            Assert.Equal(MediaStatus.UPLOADING, mediaAsset?.Status);
            Assert.NotNull(mediaAsset);
        });
        
        return startMultipartResult.Value;
    }
}