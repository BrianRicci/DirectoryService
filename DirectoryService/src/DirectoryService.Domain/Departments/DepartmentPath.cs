using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public record DepartmentPath
{
    public string Value { get; } 
    
    private DepartmentPath(string value)
    {
        Value = value;
    }
    
    public static Result<DepartmentPath> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<DepartmentPath>("Department path can't be empty or null");
        }
        
        if (!Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
        {
            return Result.Failure<DepartmentPath>("Department path can only contain numbers and latin letters");
        }
        
        if (value.Contains('.'))
        {
            return Result.Failure<DepartmentPath>($"Department path can't contain dots");
        }
        
        value = value.Trim();

        return new DepartmentPath(value);
    }
}