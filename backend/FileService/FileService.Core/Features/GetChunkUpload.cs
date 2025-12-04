using CSharpFunctionalExtensions;
using FileService.Contracts.GetChunkUpload;
using FileService.Domain;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public class GetChunkUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/url", async Task<EndpointResult<GetChunkUploadResponse>> (
            [FromBody] GetChunkUploadRequest request,
            [FromServices] GetChunkUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class GetChunkUploadHandler
{
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<GetChunkUploadHandler> _logger;

    public GetChunkUploadHandler(
        IChunkSizeCalculator chunkSizeCalculator,
        IMediaAssetsRepository mediaAssetsRepository,
        IS3Provider s3Provider,
        ILogger<GetChunkUploadHandler> logger)
    {
        _chunkSizeCalculator = chunkSizeCalculator;
        _mediaAssetsRepository = mediaAssetsRepository;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<GetChunkUploadResponse, Error>> Handle(
        GetChunkUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ChunkUploadUrl.PartNumber <= 0)
        {
            return Error.Validation(
                "part.number.less.zero",
                "The part number must be greater than zero");
        }

        var mediaAssetResult = await _mediaAssetsRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        var chunkUploadUrlsResult = await _s3Provider.GenerateChunkUploadUrlAsync(
            mediaAssetResult.Value.RawKey,
            request.ChunkUploadUrl,
            cancellationToken);
        if (chunkUploadUrlsResult.IsFailure)
            return chunkUploadUrlsResult.Error;

        _logger.LogInformation("Generated chunk upload url for media asset {MediaAssetId}", request.MediaAssetId);

        return new GetChunkUploadResponse(chunkUploadUrlsResult.Value);
    }
}