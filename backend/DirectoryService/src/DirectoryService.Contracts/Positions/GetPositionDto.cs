using System;
using System.Collections.Generic;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Contracts.Positions;

public record GetPositionDto
{
    public Guid PositionId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public List<DepartmentDto> Departments { get; init; } = [];
}