using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    private const int MIN_LENGTH = LengthConstants.LENGTH3;
    private const int MAX_LENGTH = LengthConstants.LENGTH100;
    
    public string Value { get; }
    
    private PositionName(string value)
    {
        Value = value;
    }
    
    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PositionName>("Position name can't be empty or null");
        }
        
        value = value.Trim();

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<PositionName>($"Position name must be between {MIN_LENGTH} and {MAX_LENGTH} characters");
        }

        return new PositionName(value);
    }
}