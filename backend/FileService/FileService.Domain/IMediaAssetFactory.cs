using CSharpFunctionalExtensions;
using FileService.Domain.Assets;
using Shared.SharedKernel;

namespace FileService.Domain;

public interface IMediaAssetFactory
{
    Result<PreviewAsset, Error> CreatePreviewAsset(Guid id, MediaData mediaData, MediaOwner owner);
    
    Result<VideoAsset, Error> CreateVideoAsset(Guid id, MediaData mediaData, MediaOwner owner);
}