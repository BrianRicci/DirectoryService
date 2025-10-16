using DirectoryService.Application.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres;

public class DirectoryServiceDbContext : DbContext, IReadDbContext
{
    private readonly string _connectionString;
    
    public DirectoryServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
    }
    
    private ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder => builder.AddConsole());
    }
    
    public DbSet<Department> Departments => Set<Department>();
    
    public DbSet<Location> Locations => Set<Location>();
    
    public DbSet<Position> Positions => Set<Position>();
    
    public IQueryable<Department> DepartmentsRead => Set<Department>().AsQueryable().AsNoTracking();
    
    public IQueryable<Location> LocationsRead => Set<Location>().AsQueryable().AsNoTracking();
    
    public IQueryable<Position> PositionsRead => Set<Position>().AsQueryable().AsNoTracking();
}