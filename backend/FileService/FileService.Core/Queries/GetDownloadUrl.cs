using CSharpFunctionalExtensions;
using FileService.Contracts.GetDownloadUrl;
using FileService.Contracts.GetMediaAssetInfo;
using FileService.Domain.Assets;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.SharedKernel;
using FileInfo = System.IO.FileInfo;

namespace FileService.Core.Queries;

public sealed class GetDownloadUrl : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/url", async Task<EndpointResult<GetDownloadUrlResponse>> (
            [FromBody] GetDownloadUrlRequest request,
            [FromServices] GetDownloadUrlHandler handler,
            CancellationToken token) => await handler.Handle(request, token));
    }
}

public sealed class GetDownloadUrlHandler
{
    private readonly IReadDbContext _readDbContext;
    private readonly IS3Provider _s3Provider;

    public GetDownloadUrlHandler(IReadDbContext readDbContext, IS3Provider s3Provider)
    {
        _readDbContext = readDbContext;
        _s3Provider = s3Provider;
    }

    public async Task<Result<GetDownloadUrlResponse, Error>> Handle(
        GetDownloadUrlRequest request,
        CancellationToken cancellationToken)
    {
        var mediaAsset = await _readDbContext.MediaAssetsQuery.FirstOrDefaultAsync(
            ma => ma.Id == request.MediaAssetId && ma.Status != MediaStatus.DELETED, // тут проверка статуса нужна?
            cancellationToken);
        
        if (mediaAsset == null)
            return GeneralErrors.NotFound(request.MediaAssetId);
        
        // тут тоже на статус READY?
        var downloadUrlResult = await _s3Provider.GenerateDownloadUrlAsync(mediaAsset.RawKey);
        if (downloadUrlResult.IsFailure)
            return downloadUrlResult.Error;
        
        string? downloadUrl = downloadUrlResult.Value;
        
        return new GetDownloadUrlResponse(downloadUrl);
    }
}