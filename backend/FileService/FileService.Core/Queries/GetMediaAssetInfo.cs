using CSharpFunctionalExtensions;
using FileService.Contracts.GetMediaAssetInfo;
using FileService.Domain.Assets;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.SharedKernel;
using FileInfo = FileService.Contracts.GetMediaAssetInfo.FileInfo;

namespace FileService.Core.Queries;

public sealed class GetMediaAssetInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{mediaAssetId}", async Task<EndpointResult<GetMediaAssetInfoResponse>> (
            [FromRoute] Guid mediaAssetId,
            [FromServices] GetMediaAssetInfoHandler handler,
            CancellationToken token) => await handler.Handle(mediaAssetId, token));
    }
}

public sealed class GetMediaAssetInfoHandler
{
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly IS3Provider _s3Provider;

    public GetMediaAssetInfoHandler(IMediaAssetsRepository mediaAssetsRepository, IS3Provider s3Provider)
    {
        _mediaAssetsRepository = mediaAssetsRepository;
        _s3Provider = s3Provider;
    }

    public async Task<Result<GetMediaAssetInfoResponse, Error>> Handle(
        Guid mediaAssetId,
        CancellationToken cancellationToken)
    {
        var mediaAssetResult = await _mediaAssetsRepository.GetBy(
            ma => ma.Id == mediaAssetId && ma.Status != MediaStatus.DELETED,
            cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        var mediaAsset = mediaAssetResult.Value;
        
        string? downloadUrl = null;
        
        if (mediaAsset.Status == MediaStatus.READY)
        {
            var downloadUrlResult = await _s3Provider.GenerateDownloadUrlAsync(mediaAsset.RawKey);
            if (downloadUrlResult.IsFailure)
                return downloadUrlResult.Error;
            
            downloadUrl = downloadUrlResult.Value;
        }

        FileInfo fileInfo = new FileInfo(
            mediaAsset.MediaData.FileName.Value,
            mediaAsset.MediaData.ContentType.Value,
            mediaAsset.MediaData.FileSize);
        
        return new GetMediaAssetInfoResponse(
            mediaAsset.Id,
            mediaAsset.Status.ToString(),
            mediaAsset.AssetType.ToString(),
            mediaAsset.CreatedAt,
            mediaAsset.UpdatedAt,
            fileInfo,
            downloadUrl);
    }
}