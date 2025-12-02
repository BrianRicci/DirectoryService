using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public sealed record MediaData
{
    public FileName FileName { get; }
    
    public ContentType ContentType { get; }
    
    public long FileSize { get; }
    
    public int ExpectedChunksCount { get; }

    // EF Core
    private MediaData()
    {
    }
    
    private MediaData(FileName fileName, ContentType contentType, long fileSize, int expectedChunksCount)
    {
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        ExpectedChunksCount = expectedChunksCount;
    }
    
    public static Result<MediaData, Error> Create(FileName fileName, ContentType contentType, long size, int expectedChunksCount)
    {
        if (size <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(size));
        
        if (expectedChunksCount <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(expectedChunksCount));
        
        return new MediaData(fileName, contentType, size, expectedChunksCount);
    }
}