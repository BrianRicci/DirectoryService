using System.Text.RegularExpressions;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentIdentifier
{
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 150;
    
    public DepartmentIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < MIN_LENGTH || 
            value.Length > MAX_LENGTH ||
            !Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"))
        {
            throw new ArgumentException("Invalid identifier");
        }
        
        Value = value;
    }
    
    public string Value { get; }
}