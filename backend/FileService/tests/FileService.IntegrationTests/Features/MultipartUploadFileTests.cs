using System.Net.Http.Json;
using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using FileService.Contracts.Dtos.StartMultipartUpload;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.SharedKernel;
using CompleteMultipartUploadRequest = FileService.Contracts.Dtos.CompleteMultipartUpload.CompleteMultipartUploadRequest;

namespace FileService.IntegrationTests.Features;

public class MultipartUploadFileTests : FileServiceTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;

    public MultipartUploadFileTests(IntegrationTestsWebFactory factory) 
        : base(factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task MultipartUpload_FullCycle_PersistsMediaFile()
    {
        // arrange 
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));
        
        // act
        StartMultipartUploadResponse startMultipartUploadResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        IReadOnlyList<PartETagDto> partETags = await UploadChunks(fileInfo, startMultipartUploadResponse, cancellationToken);
        
        UnitResult<Errors> result = await CompleteMultipartUpload(startMultipartUploadResponse, partETags, cancellationToken);
        
        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(ma => ma.Id == startMultipartUploadResponse.MediaAssetId, cancellationToken);
            
            Assert.Equal(MediaStatus.UPLOADED, mediaAsset?.Status);
            Assert.NotNull(mediaAsset);
            
            IAmazonS3 amazonS3Client = _factory.Services.GetRequiredService<IAmazonS3>();
            
            GetObjectResponse objectResponse = await amazonS3Client.GetObjectAsync(
                mediaAsset.RawKey.Bucket,
                mediaAsset.RawKey.Key,
                cancellationToken);
            
            Assert.Equal(objectResponse.ContentLength, fileInfo.Length);
            Assert.Equal(objectResponse.Key, mediaAsset.RawKey.Value);
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
    
    private async Task<IReadOnlyList<PartETagDto>> UploadChunks(FileInfo fileInfo, StartMultipartUploadResponse startMultipartUploadResponse, CancellationToken cancellationToken)
    {
        await using FileStream stream = fileInfo.OpenRead();

        var parts = new List<PartETagDto>();

        foreach (ChunkUploadUrl chunkUploadUrl in startMultipartUploadResponse.ChunkUploadUrls.OrderBy(c => c.PartNumber))
        {
            byte[] chunk = new byte[startMultipartUploadResponse.ChunkSize];
            
            int bytesRead = await stream.ReadAsync(chunk.AsMemory(0, startMultipartUploadResponse.ChunkSize), cancellationToken);
            if (bytesRead == 0)
                break;

            var content = new ByteArrayContent(chunk);
            
            HttpResponseMessage response = await HttpClient.PutAsync(chunkUploadUrl.UploadUrl, content, cancellationToken);
            
            string? etag = response.Headers.ETag?.Tag.Trim('"');
            
            parts.Add(new PartETagDto(chunkUploadUrl.PartNumber, etag!));
        }
        
        return parts;
    }
    
    private async Task<UnitResult<Errors>> CompleteMultipartUpload(
        StartMultipartUploadResponse startMultipartUploadResponse,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        var completeRequest = new CompleteMultipartUploadRequest(
            startMultipartUploadResponse.MediaAssetId,
            startMultipartUploadResponse.UploadId,
            partETags.ToList());
        
        HttpResponseMessage completeResponse = await AppHttpClient.PostAsJsonAsync("api/files/multipart/complete", completeRequest, cancellationToken);

        UnitResult<Errors> completeMultipart = await completeResponse.HandleResponseAsync(cancellationToken);
        
        return completeMultipart;
    }
}