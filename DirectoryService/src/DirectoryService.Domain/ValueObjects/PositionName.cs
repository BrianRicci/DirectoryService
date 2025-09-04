namespace DirectoryService.Domain.ValueObjects;

public record PositionName
{
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 100;
    
    public PositionName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < MIN_LENGTH ||
            value.Length > MAX_LENGTH) // Нужно будет добавить проверку на уникальность, от подсказок не откажусь
        {
            throw new ArgumentException("Invalid name");
        }
        
        Value = value;
    }
    
    public string Value { get; }
}