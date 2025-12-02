using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Shared.SharedKernel;

namespace FileService.Domain;

public class MediaAssetFactory : IMediaAssetFactory
{
    public Result<MediaAsset, Error> CreateForUpload(AssetType assetType, Guid id, MediaData mediaData)
    {
        return assetType switch
        {
            AssetType.VIDEO => VideoAsset.CreateForUpload(id, mediaData).Map(asset => (MediaAsset)asset),
            AssetType.PREVIEW => PreviewAsset.CreateForUpload(id, mediaData).Map(asset => (MediaAsset)asset),
            _ => GeneralErrors.ValueIsInvalid("assetType"),
        };
    }   
}