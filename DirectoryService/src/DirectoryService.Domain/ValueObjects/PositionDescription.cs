namespace DirectoryService.Domain.ValueObjects;

public record PositionDescription
{
    private const int MAX_LENGTH = 1000;
    
    public PositionDescription(string value)
    {
        if (value.Length <= MAX_LENGTH)
        {
            throw new ArgumentException("The description is too long");
        }
        
        Value = value;
    }
    
    public string Value { get; }
}