using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos.GetMediaAssetInfo;
using FileService.Contracts.Dtos.GetMediaAssetsInfo;
using Shared.SharedKernel;

namespace FileService.Contracts;

public interface IFileService
{
    Task<Result<GetMediaAssetInfoResponse, Errors>> GetMediaAssetInfo(Guid mediaAssetId, CancellationToken cancellationToken);

    Task<Result<GetMediaAssetsInfoResponse, Errors>>
        GetMediaAssetsInfo(GetMediaAssetsInfoRequest request, CancellationToken cancellationToken);
}