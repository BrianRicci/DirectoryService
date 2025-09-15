using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public record LocationTimezone
{
    public string Value { get; }
    
    private LocationTimezone(string value)
    {
        Value = value;
    }
    
    public static Result<LocationTimezone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<LocationTimezone>("Timezone can't be empty or null");
        }
        
        value = value.Trim();
        
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(value, out var _))
        {
            return Result.Failure<LocationTimezone>("Invalid IANA timezone code");
        }

        return new LocationTimezone(value);
    }
}