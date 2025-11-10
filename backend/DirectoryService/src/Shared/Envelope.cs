using System.Text.Json.Serialization;

namespace Shared;

public record Envelope
{
    public object? Result { get; }
    
    public Errors? ErrorList { get; }
    
    public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());
    
    public DateTime TimeGenerated { get; }
    
    [JsonConstructor]
    public Envelope(object? result, Errors? errorList)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = DateTime.UtcNow;
    }
    
    public static Envelope Ok(object? result = null) =>
        new(result, null);
    
    public static Envelope Error(Errors errors) =>
        new(null, errors);
}

public record Envelope<T>
{
    public T? Result { get; }
    
    public Errors? ErrorList { get; }
    
    public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());
    
    public DateTime TimeGenerated { get; }
    
    [JsonConstructor]
    public Envelope(T? result, Errors? errorList)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = DateTime.UtcNow;
    }
    
    public static Envelope<T> Ok(T? result = default) =>
        new(result, null);
    
    public static Envelope<T> Error(Errors errors) =>
        new(default, errors);
}