using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos.DeleteFile;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.SharedKernel;

namespace FileService.Core.Command;

public sealed class DeleteFile: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/files/{mediaAssetId}", async Task<EndpointResult<DeleteFileResponse>> (
            [FromRoute] Guid mediaAssetId,
            [FromServices] DeleteFileHandler handler,
            CancellationToken token) => await handler.Handle(mediaAssetId, token));
    }
}

public sealed class DeleteFileHandler
{
    private readonly IMediaAssetsRepository _mediaAssetsRepository;
    private readonly IS3Provider _s3Provider;

    public DeleteFileHandler(IMediaAssetsRepository mediaAssetsRepository, IS3Provider s3Provider)
    {
        _mediaAssetsRepository = mediaAssetsRepository;
        _s3Provider = s3Provider;
    }

    public async Task<Result<DeleteFileResponse, Error>> Handle(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        var mediaAssetResult = await _mediaAssetsRepository.GetByIdAsync(mediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        var mediaAsset = mediaAssetResult.Value;

        mediaAsset.MarkDeleted(DateTime.UtcNow);

        await _mediaAssetsRepository.SaveChangesAsync(cancellationToken);
        
        var deleteFileResult = await _s3Provider.DeleteFileAsync(mediaAsset.RawKey, cancellationToken);
        if (deleteFileResult.IsFailure)
            return deleteFileResult.Error;
        
        return new DeleteFileResponse(mediaAssetId);
    }
}