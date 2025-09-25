using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    private const int MIN_LENGTH = LengthConstants.LENGTH3;
    private const int MAX_LENGTH = LengthConstants.LENGTH120;
    
    public string Value { get; }
    
    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Location name can't be empty or null");
        }
        
        value = value.Trim();

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid($"Location name must be between {MIN_LENGTH} and {MAX_LENGTH} characters");
        }

        return new LocationName(value);
    }
}