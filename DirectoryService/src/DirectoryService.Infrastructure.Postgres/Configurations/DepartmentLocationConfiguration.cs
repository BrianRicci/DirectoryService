using DirectoryService.Domain.Departments;
using DirectoryService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");
        
        builder.HasKey(dl => dl.Id).HasName("pk_department_locations");
        
        builder
            .Property(dl => dl.Id)
            .HasConversion(dl => dl.Value, id => new DepartmentLocationId(id))
            .HasColumnName("department_location_id");
    }
}