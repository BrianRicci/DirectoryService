using CSharpFunctionalExtensions;
using FileService.Core;
using Microsoft.Extensions.Options;
using Shared.SharedKernel;

namespace FileService.Infrastructure.S3;

public class ChunkSizeCalculator : IChunkSizeCalculator
{
    private const int MIN_CHUNK_COUNT = 1;
    private readonly S3Options _s3Options;

    public ChunkSizeCalculator(IOptions<S3Options> s3Options)
    {
        _s3Options = s3Options.Value;
    }
    
    public Result<(int ChunkSize, int TotalChunks), Error> Calculate(long fileSize)
    {
        if (_s3Options.RecommendedChunkSizeBytes <= 0 || _s3Options.MaxChunks <= 0)
            return GeneralErrors.ValueIsInvalid("настройки чанков");

        if (fileSize <= _s3Options.RecommendedChunkSizeBytes)
        {
            return ((int)fileSize, MIN_CHUNK_COUNT);
        }
        
        int calculatedChunks = (int)Math.Ceiling((double)fileSize / _s3Options.RecommendedChunkSizeBytes);

        int actualChunks = Math.Min(calculatedChunks, _s3Options.MaxChunks);
        
        int chunkSize = (int)(fileSize + actualChunks - 1) / actualChunks;
        
        return (chunkSize, actualChunks);
    }
}