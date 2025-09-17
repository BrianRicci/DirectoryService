using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions;

public record PositionDescription
{
    private const int MAX_LENGTH = LengthConstants.LENGTH1000;
    
    public string Value { get; }
    
    private PositionDescription(string value)
    {
        Value = value;
    }
    
    public static Result<PositionDescription> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new PositionDescription(string.Empty);
        }
        
        value = value.Trim();
        
        if (value.Length <= MAX_LENGTH)
        {
            return Result.Failure<PositionDescription>("Position description is too long");
        }

        return new PositionDescription(value);
    }
}