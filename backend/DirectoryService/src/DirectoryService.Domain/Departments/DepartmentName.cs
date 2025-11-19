using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    private const int MIN_LENGTH = Constants.LENGTH3;
    private const int MAX_LENGTH = Constants.LENGTH150;
    
    public string Value { get; }
    
    private DepartmentName(string value)
    {
        Value = value;
    }
    
    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Department name can't be empty or null");
        }
        
        value = value.Trim();

        if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid($"Department name must be between {MIN_LENGTH} and {MAX_LENGTH} characters");
        }

        return new DepartmentName(value);
    }
}