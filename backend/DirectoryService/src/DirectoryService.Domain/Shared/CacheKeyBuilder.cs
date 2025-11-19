using System.Text;

namespace DirectoryService.Domain.Shared;

public static class CacheKeyBuilder
{
    public static string Build(string prefix, params (string Name, object Value)[] parameters)
    {
        var key = new StringBuilder(prefix);
        
        foreach ((string name, object value) in parameters)
        {
            key.Append($"_{name}_{value}");
        }

        return key.ToString();
    }
}