using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public class Department
{
    public DepartmentId Id { get; private set; }
    
    public DepartmentName Name { get; private set; }
    
    public DepartmentIdentifier Identifier { get; private set; }
    
    public Guid? ParentId { get; private set; }
    
    public DepartmentPath Path { get; private set; } 
    
    public short Depth { get; private set; }
    
    public bool IsActive { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _locations = [];

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _positions = [];
    
    private List<DepartmentLocation> _locations;
    
    private List<DepartmentPosition> _positions;
    
    // EF Core
    private Department()
    {
    }
    
    private Department(
        DepartmentId id,
        DepartmentName name,
        DepartmentIdentifier identifier,
        Guid? parentId,
        List<DepartmentLocation> departmentLocations)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        UpdatedAt = DateTime.UtcNow;
        _locations = departmentLocations;

        if (CreatedAt == default)
        {
            CreatedAt = DateTime.UtcNow;
        }
    }

    public static Result<Department> Create(
        string name,
        string identifier,
        Guid? parentId,
        List<DepartmentLocation> departmentLocations)
    {
        Result<DepartmentName> nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure<Department>(nameResult.Error);

        Result<DepartmentIdentifier> identifierResult = DepartmentIdentifier.Create(identifier);
        if (identifierResult.IsFailure)
            return Result.Failure<Department>(identifierResult.Error);
        
        var id = new DepartmentId(Guid.NewGuid());
        
        return new Department(id, nameResult.Value, identifierResult.Value, parentId, departmentLocations);
    }
    
    public Result Rename(string name)
    {
        Result<DepartmentName> nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
            return Result.Failure<Department>(nameResult.Error);
        
        Name = nameResult.Value;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Success(this);
    }
}