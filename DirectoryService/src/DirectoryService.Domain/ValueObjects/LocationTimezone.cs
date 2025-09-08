namespace DirectoryService.Domain.ValueObjects;

public record LocationTimezone
{
    public LocationTimezone(string value)
    {
        try
        {
            Value = TimeZoneInfo.FindSystemTimeZoneById(value);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException("Timezone not found");
        }
        catch (InvalidTimeZoneException)
        {
            throw new ArgumentException("Invalid timezone");
        }
    }
    
    public TimeZoneInfo Value { get; }
}