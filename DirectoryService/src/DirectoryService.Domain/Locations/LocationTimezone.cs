using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Locations;

public record LocationTimezone
{
    public string Value { get; }
    
    private LocationTimezone(string value)
    {
        Value = value;
    }
    
    public static Result<LocationTimezone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired(value);
        }
        
        value = value.Trim();

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(value, out var _))
        {
            return GeneralErrors.ValueIsInvalid(value);
        }
        
        return new LocationTimezone(value);
    }
}