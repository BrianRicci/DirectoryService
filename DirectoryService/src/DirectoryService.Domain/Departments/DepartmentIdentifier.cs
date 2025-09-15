using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public record DepartmentIdentifier
{
    private const int MIN_LENGTH = LengthConstants.LENGTH3;
    private const int MAX_LENGTH = LengthConstants.LENGTH150;
    
    public string Value { get; }
    
    private DepartmentIdentifier(string value)
    {
        Value = value;
    }
    
    public static Result<DepartmentIdentifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DepartmentIdentifier>("Department identifier can't be empty or null");
        }
        
        if (!Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
        {
            return Result.Failure<DepartmentIdentifier>("Department identifier can only contain numbers and latin letters");
        }
        
        value = value.Trim();

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return Result.Failure<DepartmentIdentifier>($"Department identifier must be between {MIN_LENGTH} and {MAX_LENGTH} characters");
        }

        return new DepartmentIdentifier(value);
    }
}