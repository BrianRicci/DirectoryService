using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain.Assets;

public class PreviewAsset : MediaAsset
{
    public const long MAX_SIZE = 10_485_760; // 10 MB
    public const string BUCKET = "preview";
    public const string RAW_PREFIX = "raw";
    public const string ALLOWED_CONTENT_TYPE = "preview";

    public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];
    
    // EF Core
    private PreviewAsset()
    {
    }
    
    private PreviewAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        StorageKey key)
        : base(id, mediaData, status, AssetType.PREVIEW, key)
    {
    }

    public static UnitResult<Error> ValidateForUpload(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation(
                "preview.invalid.extension",
                $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
        }
        
        if (mediaData.ContentType.Category != MediaType.IMAGE)
        {
            return Error.Validation(
                "preview.invalid.content-type",
                $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }
        
        if (mediaData.FileSize > MAX_SIZE)
        {
            return Error.Validation(
                "preview.invalid.size",
                $"File size must be less than {MAX_SIZE} bytes");
        }
        
        return UnitResult.Success<Error>();
    }
    
    public static Result<PreviewAsset, Error> CreateForUpload(Guid id, MediaData mediaData)
    {
        UnitResult<Error> validationResult = ValidateForUpload(mediaData);
        if (validationResult.IsFailure)
            return validationResult.Error;
        
        Result<StorageKey, Error> key = StorageKey.Create(BUCKET, null, id.ToString());
        if (key.IsFailure)
            return key.Error;
        
        return new PreviewAsset(id, mediaData, MediaStatus.UPLOADING, key.Value);
    }
    
    public UnitResult<Error> CompleteProcessing(DateTime timestamp)
    {
        MarkUploaded(timestamp);
        MarkReady(RawKey, timestamp);
        
        return UnitResult.Success<Error>();
    }
}