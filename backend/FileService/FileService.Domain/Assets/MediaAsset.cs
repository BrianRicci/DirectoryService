using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.Assets;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }

    public MediaData MediaData { get; protected set; } = null!;

    public AssetType AssetType { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    public StorageKey RawKey { get; protected set; } = null!;
    
    public StorageKey FinalKey { get; protected set; } = null!;

    // TODO
    // НЕ ЗАБЫТЬ РАСКОММЕНТИТЬ ПОТОМ
    // public MediaOwner Owner { get; protected set; } = null!;
    
    public MediaStatus Status { get; protected set; }

    // EF core
    protected MediaAsset()
    {
    }

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType assetType,
        StorageKey rawKey)
    {
        Id = id;
        MediaData = mediaData;
        Status = status;
        AssetType = assetType;
        RawKey = rawKey;
        FinalKey = rawKey; // TODO: НЕ ЗАБЫТЬ ПОМЕНЯТЬ!
    }
    
    public UnitResult<Error> MarkUploaded(DateTime timestamp)
    {
        if (Status != MediaStatus.UPLOADING)
            return GeneralErrors.ValueIsInvalid(nameof(Status));
        
        Status = MediaStatus.UPLOADED;
        UpdatedAt = timestamp;
        
        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> MarkReady(StorageKey finalyKey, DateTime timestamp)
    {
        if (Status != MediaStatus.UPLOADED || Status != MediaStatus.UPLOADING)
            return GeneralErrors.ValueIsInvalid(nameof(Status));
        
        Status = MediaStatus.READY;
        UpdatedAt = timestamp;
        FinalKey = finalyKey;
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> MarkFailed(DateTime timestamp)
    {
        if (Status == MediaStatus.UPLOADED)
            return GeneralErrors.ValueIsInvalid(nameof(Status));
        
        Status = MediaStatus.FAILED;
        UpdatedAt = timestamp;
        
        return UnitResult.Success<Error>();
    }
    
    // Для soft delete скорее всего придется добавить потом IsDeleted и DeletedAt
    public UnitResult<Error> MarkDeleted(DateTime timestamp)
    {
        Status = MediaStatus.DELETED;
        UpdatedAt = timestamp;
        
        return UnitResult.Success<Error>();
    }
}

public enum MediaStatus
{
    UPLOADING,
    UPLOADED,
    READY,
    FAILED,
    DELETED,
}