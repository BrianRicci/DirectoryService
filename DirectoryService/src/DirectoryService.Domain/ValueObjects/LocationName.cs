namespace DirectoryService.Domain.ValueObjects;

public record LocationName
{
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 120;
    
    public LocationName(string value)
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