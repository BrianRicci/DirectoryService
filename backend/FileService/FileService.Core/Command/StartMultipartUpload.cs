using CSharpFunctionalExtensions;
using FileService.Contracts.StartMultipartUpload;
using FileService.Domain;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Command;

public sealed class StartMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/start", async Task<EndpointResult<StartMultipartUploadResponse>> (
            [FromBody] StartMultipartUploadRequest request,
            [FromServices] StartMultipartUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class StartMultipartUploadHandler
{
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<StartMultipartUploadHandler> _logger;

    public StartMultipartUploadHandler(
        IChunkSizeCalculator chunkSizeCalculator,
        IMediaAssetsRepository mediaAssetsRepository,
        IS3Provider s3Provider,
        ILogger<StartMultipartUploadHandler> logger)
    {
        _chunkSizeCalculator = chunkSizeCalculator;
        _mediaAssetsRepository = mediaAssetsRepository;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<StartMultipartUploadResponse, Error>> Handle(
        StartMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var fileNameResult = FileName.Create(request.FileName);
        if (fileNameResult.IsFailure)
            return fileNameResult.Error;

        var contentTypeResult = ContentType.Create(request.ContentType);
        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error;

        Result<(int ChunkSize, int TotalChunks), Error> chunkCalculationResult =
            _chunkSizeCalculator.Calculate(request.FileSize);
        if (chunkCalculationResult.IsFailure)
            return chunkCalculationResult.Error;

        var mediaDataResult = MediaData.Create(
            fileNameResult.Value,
            contentTypeResult.Value,
            request.FileSize,
            chunkCalculationResult.Value.TotalChunks);
        if (mediaDataResult.IsFailure)
            return mediaDataResult.Error;

        var assetType = request.AssetType.ToAssetType();

        var mediaAssetResult = MediaAssetFactory.CreateForUpload(assetType, Guid.NewGuid(), mediaDataResult.Value);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        var startUploadResult = await _s3Provider.StartMultipartUploadAsync(
            mediaAssetResult.Value.RawKey, mediaDataResult.Value, cancellationToken);
        if (startUploadResult.IsFailure)
            return startUploadResult.Error;

        await _mediaAssetsRepository.AddAsync(mediaAssetResult.Value, cancellationToken);

        var chunkUploadUrlsResult = await _s3Provider.GenerateAllChunksUploadUrlsAsync(
            mediaAssetResult.Value.RawKey,
            startUploadResult.Value,
            chunkCalculationResult.Value.TotalChunks,
            cancellationToken);
        if (chunkUploadUrlsResult.IsFailure)
            return chunkUploadUrlsResult.Error;

        _logger.LogInformation(
            "Started multipart upload for media asset {MediaAssetId} with key: {Key}",
            mediaAssetResult.Value.Id,
            mediaAssetResult.Value.RawKey);

        return new StartMultipartUploadResponse(
            mediaAssetResult.Value.Id,
            startUploadResult.Value,
            chunkUploadUrlsResult.Value,
            chunkCalculationResult.Value.ChunkSize);
    }
}