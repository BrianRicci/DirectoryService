namespace DirectoryService.Domain.ValueObjects;

public record PositionName
{
    private const int MIN_LENGTH = LengthConstants.LENGTH3;
    private const int MAX_LENGTH = LengthConstants.LENGTH100;
    
    public PositionName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < MIN_LENGTH ||
            value.Length > MAX_LENGTH)
        {
            throw new ArgumentException("Invalid name");
        }
        
        Value = value;
    }
    
    public string Value { get; }
}