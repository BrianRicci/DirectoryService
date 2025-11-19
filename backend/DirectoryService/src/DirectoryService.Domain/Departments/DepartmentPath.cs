using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Departments;

public record DepartmentPath
{
    public string Value { get; } 
    
    private DepartmentPath(string value)
    {
        Value = value;
    }

    public static Result<DepartmentPath, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("Department path can't be empty or null");
        }
        
        if (!Regex.IsMatch(value, @"^[a-zA-Z0-9._]+$"))
        {
            return GeneralErrors.ValueIsInvalid("Department path can only contain numbers and latin letters");
        }
        
        value = value.Trim();

        return new DepartmentPath(value);
    }
    
    public static Result<DepartmentPath, Error> CreateParent(DepartmentIdentifier identifier)
    {
        return DepartmentPath.Create(identifier.Value.Trim());
    }

    public Result<DepartmentPath, Error> CreateChild(DepartmentIdentifier identifier)
    {
        return DepartmentPath.Create(Value + Constants.SEPARATOR + identifier.Value.Trim());
    }

    public Result<DepartmentPath, Error> AddDeletedStatus()
    {
        if (Value.StartsWith(Constants.DELETED_PREFIX))
        {
            return GeneralErrors.ValueIsInvalid("Department path already starts with 'deleted_");
        }
        
        return DepartmentPath.Create(Constants.DELETED_PREFIX + Value);
    }

    public Result<DepartmentPath, Error> RemoveDeletedStatus()
    {
        if (!Value.StartsWith(Constants.DELETED_PREFIX))
        {
            return GeneralErrors.ValueIsInvalid("Department path doesn't start with 'deleted_");
        }
        
        return DepartmentPath.Create(Value.Replace(Constants.DELETED_PREFIX, string.Empty));
    }
}