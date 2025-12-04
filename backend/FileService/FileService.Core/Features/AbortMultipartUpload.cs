using CSharpFunctionalExtensions;
using FileService.Contracts.AbortMultipartUpload;
using FileService.Domain.Assets;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public class AbortMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/abort", async Task<EndpointResult<AbortMultipartUploadResponse>>(
            [FromBody] AbortMultipartUploadRequest request,
            [FromServices] AbortMultipartUploadHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class AbortMultipartUploadHandler
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly ILogger<AbortMultipartUploadHandler> _logger;

    public AbortMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaAssetsRepository mediaAssetsRepository,
        ILogger<AbortMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaAssetsRepository = mediaAssetsRepository;
        _logger = logger;
    }

    public async Task<Result<AbortMultipartUploadResponse, Error>> Handle(AbortMultipartUploadRequest request, CancellationToken cancellationToken)
    {
        var mediaAssetResult = await _mediaAssetsRepository.GetByIdAsync(request.MediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        MediaAsset mediaAsset = mediaAssetResult.Value;
        
        var abortResult = await _s3Provider.AbortMultipartUploadAsync(mediaAsset.RawKey, request.UploadId, cancellationToken);
        if (abortResult.IsFailure)
            return abortResult.Error;

        mediaAsset.MarkFailed(DateTime.UtcNow);

        await _mediaAssetsRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Aborted multipart upload for key: {Key}", mediaAsset.RawKey);

        return new AbortMultipartUploadResponse(abortResult.IsSuccess);
    }
}
