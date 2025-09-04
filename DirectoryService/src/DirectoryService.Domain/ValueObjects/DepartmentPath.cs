using System.Text.RegularExpressions;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentPath
{
    public DepartmentPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Contains('.') ||
            !Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
        {
            throw new ArgumentException("Invalid path");
        }
        
        Value = value;
    }
    
    public string Value { get; } 
}