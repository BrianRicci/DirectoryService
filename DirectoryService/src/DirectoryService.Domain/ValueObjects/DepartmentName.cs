namespace DirectoryService.Domain.ValueObjects;

public record DepartmentName
{
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 150;
    
    public DepartmentName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
        {
            throw new ArgumentException("Invalid name");
        }
        
        Value = value;
    }
    
    public string Value { get; }
}