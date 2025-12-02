using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Shared.SharedKernel;

namespace FileService.Domain;

public interface IMediaAssetFactory
{
    Result<MediaAsset, Error> CreateForUpload(AssetType assetType, Guid id, MediaData mediaData);
}