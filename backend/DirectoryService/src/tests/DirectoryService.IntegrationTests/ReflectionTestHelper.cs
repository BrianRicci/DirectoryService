using System.Reflection;

namespace DirectoryService.IntegrationTests;

public static class ReflectionTestHelper
{
    public static void SetPrivateProperty<T>(this T obj, string propertyName, object value)
    {
        var propertyInfo = typeof(T).GetProperty(
            propertyName, 
            BindingFlags.Public | BindingFlags.Instance);
        
        if (propertyInfo == null)
        {
            throw new ArgumentException($"Property '{propertyName}' not found on type {typeof(T).Name}");
        }
        
        propertyInfo.SetValue(obj, value);
    }
}