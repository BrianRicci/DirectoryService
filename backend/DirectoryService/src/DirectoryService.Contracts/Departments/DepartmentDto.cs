using System;

namespace DirectoryService.Contracts.Departments;

public record DepartmentDto
{
    public Guid DepartmentId { get; init; }
    
    public string Name { get; init; } = string.Empty;
    
    public string Identifier { get; init; } = string.Empty;
    
    public Guid? ParentId { get; init; }
    
    public string Path { get; init; } = string.Empty;
    
    public short Depth { get; init; }
    
    public bool IsActive { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime UpdatedAt { get; init; }
}