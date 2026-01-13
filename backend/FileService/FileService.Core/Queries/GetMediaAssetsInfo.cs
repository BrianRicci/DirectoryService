using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos.GetMediaAssetsInfo;
using FileService.Domain;
using FileService.Domain.Assets;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;

namespace FileService.Core.Queries;

public sealed class GetMediaAssetsInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/batch", async Task<EndpointResult<GetMediaAssetsInfoResponse>> (
            [FromBody] GetMediaAssetsInfoRequest request,
            [FromServices] GetMediaAssetsInfoHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class GetMediaAssetsInfoHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly IS3Provider _s3Provider;

    public GetMediaAssetsInfoHandler(IReadDbContext readDbContext, IS3Provider s3Provider)
    {
        _readDbContext = readDbContext;
        _s3Provider = s3Provider;
    }

    public async Task<Result<GetMediaAssetsInfoResponse, Error>> Handle(
        GetMediaAssetsInfoRequest request,
        CancellationToken cancellationToken)
    {
        if (request.MediaAssetIds.Count == 0)
            return GeneralErrors.ValueIsRequired("mediaAssetIds");
        
        var mediaAssets = await _readDbContext.MediaAssetsQuery
            .Where(ma => request.MediaAssetIds.Contains(ma.Id) && ma.Status != MediaStatus.DELETED)
            .ToListAsync(cancellationToken);
        
        List<StorageKey> keys = mediaAssets
            .Where(ma => ma.Status == MediaStatus.READY)
            .Select(ma => ma.RawKey)
            .ToList();
        
        var downloadUrlsResult = await _s3Provider.GenerateDownloadUrlsAsync(keys);
        if (downloadUrlsResult.IsFailure)
            return downloadUrlsResult.Error;
        
        return new GetMediaAssetsInfoResponse(
            mediaAssets.Select(ma => new GetMediaAssetInfoForBatch(
                ma.Id,
                ma.Status.ToString(),
                downloadUrlsResult.Value.TryGetValue(ma.RawKey, out string? downloadUrl) ? downloadUrl : null)));;
    }
}