using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public sealed record StorageKey
{
    public string Key { get; }
    
    public string Prefix { get; }
    
    public string Bucket { get; }
    
    public string Value { get; }
    
    public string FullPath { get; private set; }
    
    private StorageKey(string bucket, string prefix, string key)
    {
        Bucket = bucket;
        Prefix = prefix;
        Key = key;
        Value = string.IsNullOrEmpty(Prefix) ? key : $"{Prefix}/{key}";
        FullPath = $"{Bucket}/{Value}";
    }
    
    public static Result<StorageKey, Error> Create(string location, string? prefix, string key)
    {
        if (string.IsNullOrWhiteSpace(location))
            return GeneralErrors.ValueIsInvalid(nameof(location));
        
        Result<string, Error> normalizedKeyResult = NormalizeSegment(key);
        if (normalizedKeyResult.IsFailure)
            return normalizedKeyResult.Error;
        
        Result<string, Error> normalizedPrefixResult = NormalizePrefix(prefix);
        if (normalizedPrefixResult.IsFailure)
            return normalizedPrefixResult.Error;
        
        return new StorageKey(location.Trim(), normalizedPrefixResult.Value, normalizedKeyResult.Value);
    }
    
    public UnitResult<Error> AppendSegment(string value)
    {
        Result<string, Error> normalizedValue = NormalizeSegment(value);
        if (normalizedValue.IsFailure)
            return normalizedValue.Error;
        
        FullPath = $"{FullPath}/{normalizedValue.Value}";
        
        return UnitResult.Success<Error>();
    }
    
    private static Result<string, Error> NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return string.Empty;
        
        string[] parts = prefix.Trim().Replace("\\", "/").Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        List<string> normalizedParts = [];
        foreach (string part in parts)
        {
            Result<string, Error> normalizedPart = NormalizeSegment(part);
            if (normalizedPart.IsFailure)
                return normalizedPart;
            
            if (!string.IsNullOrEmpty(normalizedPart.Value))
                normalizedParts.Add(normalizedPart.Value);
        }
        
        return string.Join('/', normalizedParts);
    }

    private static Result<string, Error> NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsInvalid("key");

        string trimmed = value.Trim();
        
        if (trimmed.Contains('/', StringComparison.Ordinal) || trimmed.Contains('\\', StringComparison.Ordinal))
            return GeneralErrors.ValueIsInvalid("key");
        
        return trimmed;
    }
}