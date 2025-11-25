using CSharpFunctionalExtensions;
using FileService.Domain;
using FileService.Domain.Assets;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IMediaAssetsRepository
{
    Task<Result<Guid, Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken);
}