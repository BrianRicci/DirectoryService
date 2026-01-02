using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.DeleteFile;
using FileService.Contracts.StartMultipartUpload;
using FileService.Domain.Assets;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;
using CompleteMultipartUploadRequest = FileService.Contracts.CompleteMultipartUpload.CompleteMultipartUploadRequest;

namespace FileService.IntegrationTests.Features;

public class DeleteFileTests : FileServiceTestsBase
{
    private readonly IntegrationTestsWebFactory _factory;

    public DeleteFileTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteFile_DeletesFile()
    {
        // arrange 
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        StartMultipartUploadResponse startMultipartUploadResponse =
            await StartMultipartUpload(fileInfo, cancellationToken);

        IReadOnlyList<PartETagDto> partETags =
            await UploadChunks(fileInfo, startMultipartUploadResponse, cancellationToken);

        await CompleteMultipartUpload(startMultipartUploadResponse, partETags, cancellationToken);

        Guid mediaAssetId = startMultipartUploadResponse.MediaAssetId;

        // act
        HttpResponseMessage deleteFileResponse =
            await AppHttpClient.DeleteAsync($"api/files/{mediaAssetId}", cancellationToken);

        Result<DeleteFileResponse, Errors> deleteFileResult =
            await deleteFileResponse.HandleResponseAsync<DeleteFileResponse>(cancellationToken);

        // assert
        Assert.True(deleteFileResult.IsSuccess);

        await ExecuteInDb(async db =>
        {
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(ma => ma.Id == startMultipartUploadResponse.MediaAssetId, cancellationToken);

            Assert.Equal(MediaStatus.DELETED, mediaAsset?.Status);
            Assert.NotNull(mediaAsset);

            // можно ли как то проверить тут есть ли в S3 хранилище этот файл
        });
    }

    private async Task<StartMultipartUploadResponse> StartMultipartUpload(FileInfo fileInfo,
        CancellationToken cancellationToken)
    {
        // arrange
        var request = new StartMultipartUploadRequest(fileInfo.Name, "video/mp4", fileInfo.Length, "video");

        // act
        HttpResponseMessage startMultipartResponse =
            await AppHttpClient.PostAsJsonAsync("api/files/multipart/start", request, cancellationToken);

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

    private async Task<IReadOnlyList<PartETagDto>> UploadChunks(FileInfo fileInfo,
        StartMultipartUploadResponse startMultipartUploadResponse, CancellationToken cancellationToken)
    {
        await using FileStream stream = fileInfo.OpenRead();

        var parts = new List<PartETagDto>();

        foreach (ChunkUploadUrl chunkUploadUrl in
                 startMultipartUploadResponse.ChunkUploadUrls.OrderBy(c => c.PartNumber))
        {
            byte[] chunk = new byte[startMultipartUploadResponse.ChunkSize];

            int bytesRead = await stream.ReadAsync(chunk.AsMemory(0, startMultipartUploadResponse.ChunkSize),
                cancellationToken);
            if (bytesRead == 0)
                break;

            var content = new ByteArrayContent(chunk);

            HttpResponseMessage response =
                await HttpClient.PutAsync(chunkUploadUrl.UploadUrl, content, cancellationToken);

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

        HttpResponseMessage completeResponse =
            await AppHttpClient.PostAsJsonAsync("api/files/multipart/complete", completeRequest, cancellationToken);

        UnitResult<Errors> completeMultipart = await completeResponse.HandleResponseAsync(cancellationToken);

        return completeMultipart;
    }
}