using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Departments;

public record DepartmentIdentifier
{
    private const int MIN_LENGTH = Constants.LENGTH3;
    private const int MAX_LENGTH = Constants.LENGTH150;
    
    public string Value { get; }
    
    private DepartmentIdentifier(string value)
    {
        Value = value;
    }
    
    public static Result<DepartmentIdentifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Department identifier can't be empty or null");
        }

        if (!Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
        {
            return GeneralErrors.ValueIsInvalid("Department identifier can only contain numbers and latin letters");
        }
        
        if (value.Contains('.'))
        {
            return GeneralErrors.ValueIsInvalid($"Department identifier can't contain dots");
        }
        
        value = value.Trim();

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid($"Department identifier must be between {MIN_LENGTH} and {MAX_LENGTH} characters");
        }

        return new DepartmentIdentifier(value);
    }
}