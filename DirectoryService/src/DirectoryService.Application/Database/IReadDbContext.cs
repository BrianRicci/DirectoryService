using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Database;

public interface IReadDbContext
{
    IQueryable<Department> DepartmentsRead { get; }
    
    IQueryable<Location> LocationsRead { get; }
    
    IQueryable<Position> PositionsRead { get; }
}