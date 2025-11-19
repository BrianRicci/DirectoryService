using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Positions;

public record PositionDescription
{
    private const int MAX_LENGTH = Constants.LENGTH1000;
    
    public string Value { get; }
    
    private PositionDescription(string value)
    {
        Value = value;
    }
    
    public static Result<PositionDescription, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new PositionDescription(string.Empty);
        }
        
        value = value.Trim();
        
        if (value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("Position description is too long");
        }

        return new PositionDescription(value);
    }
}