using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.GetChunkUpload;
using FileService.Contracts.StartMultipartUpload;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;
using AbortMultipartUploadRequest = FileService.Contracts.AbortMultipartUpload.AbortMultipartUploadRequest;
using CompleteMultipartUploadRequest = FileService.Contracts.CompleteMultipartUpload.CompleteMultipartUploadRequest;

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
            
            // можно ли как то проверить тут есть ли в S3 хранилище этот файл
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