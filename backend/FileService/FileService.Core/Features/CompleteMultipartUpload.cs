using CSharpFunctionalExtensions;
using FileService.Contracts.CompleteMultipartUpload;
using FileService.Domain.Assets;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public sealed class CompleteMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/complete", async Task<EndpointResult<CompleteMultipartUploadResponse>> (
            [FromBody] CompleteMultipartUploadRequest request,
            [FromServices] CompleteMultipartUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class CompleteMultipartUploadHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly ILogger<CompleteMultipartUploadHandler> _logger;

    public CompleteMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository,
        ILogger<CompleteMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
        _logger = logger;
    }

    public async Task<Result<CompleteMultipartUploadResponse, Error>> Handle(
        CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var mediaAssetResult = await _mediaAssetsRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        MediaAsset mediaAsset = mediaAssetResult.Value;

        if (request.PartETags.Count != mediaAsset.MediaData.ExpectedChunksCount)
        {
            return Error.Validation(
                "part.etags.count.mismatch",
                "The number of part ETags does not match the expected number of chunks");
        }

        var completeMultipartUploadResult = await _s3Provider.CompleteMultipartUploadAsync(
            mediaAsset.RawKey,
            request.UploadId,
            request.PartETags,
            cancellationToken);
        if (completeMultipartUploadResult.IsFailure)
            return completeMultipartUploadResult.Error;

        mediaAsset.MarkUploaded(DateTime.UtcNow);

        await _mediaAssetsRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Completed multipart upload for media asset {MediaAssetId}", mediaAsset.Id);

        return new CompleteMultipartUploadResponse(mediaAsset.Id);
    }
}