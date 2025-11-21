using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public sealed record FileName
{
    public string Value { get; }
    
    public string Extension { get; }
    
    private FileName(string value, string extension)
    {
        Value = value;
        Extension = Path.GetExtension(value);
    }

    public static Result<FileName, Error> Create(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return GeneralErrors.ValueIsInvalid(nameof(filename));
        
        int lastDot = filename.LastIndexOf('.');
        if (lastDot == -1 || lastDot == filename.Length - 1)
            return GeneralErrors.ValueIsInvalid(nameof(filename));

        string extension = filename[(lastDot + 1)..].ToLowerInvariant();
        return new FileName(filename, extension);
    }
}