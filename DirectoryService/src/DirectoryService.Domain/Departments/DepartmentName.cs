using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    private const int MIN_LENGTH = LengthConstants.LENGTH3;
    private const int MAX_LENGTH = LengthConstants.LENGTH150;
    
    public string Value { get; }
    
    private DepartmentName(string value)
    {
        Value = value;
    }
    
    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DepartmentName>("Department name can't be empty or null");
        }
        
        value = value.Trim();

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<DepartmentName>($"Department name must be between {MIN_LENGTH} and {MAX_LENGTH} characters");
        }

        return new DepartmentName(value);
    }
}