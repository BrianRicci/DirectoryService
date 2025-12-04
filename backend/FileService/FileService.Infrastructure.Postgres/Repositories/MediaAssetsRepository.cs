using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain;
using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Infrastructure.Postgres.Repositories;

public class MediaAssetsRepository : IMediaAssetsRepository
{
    private readonly FileServiceDbContext _dbContext;
    private readonly ILogger<MediaAssetsRepository> _logger;

    public MediaAssetsRepository(FileServiceDbContext dbContext, ILogger<MediaAssetsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.MediaAssets.AddAsync(mediaAsset, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return mediaAsset.Id;
        }
        catch (Exception ex)
        {
            return GeneralErrors.ValueIsInvalid();
        }
    }
    
    public async Task<Result<MediaAsset, Error>> GetByIdAsync(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        var mediaAsset = await _dbContext.MediaAssets.FirstOrDefaultAsync(ma => ma.Id == mediaAssetId, cancellationToken);
        
        if (mediaAsset is null)
            return GeneralErrors.NotFound(mediaAssetId);

        return mediaAsset;
    }
}