using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IChunkSizeCalculator
{
    Result<(int ChunkSize, int TotalChunks), Error> Calculate(long fileSize);
}