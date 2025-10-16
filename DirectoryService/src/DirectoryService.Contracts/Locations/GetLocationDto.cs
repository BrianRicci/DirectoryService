using System;
using System.Collections.Generic;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Contracts.Locations;

public record GetLocationDto
{
    public Guid LocationId { get; init; }

    public string Name { get; init; } = string.Empty;
    
    public required LocationAddressDto Address { get; set; }

    public string Timezone { get; init; } = string.Empty;

    public bool IsActive { get; init; }
    
    public DateTime CreatedAt { get; init; }
    
    public DateTime UpdatedAt { get; init; }

    public List<DepartmentDto> Departments { get; init; } = [];
}