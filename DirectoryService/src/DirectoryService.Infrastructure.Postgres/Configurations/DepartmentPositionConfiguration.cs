using DirectoryService.Domain.Departments;
using DirectoryService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");
        
        builder.HasKey(dp => dp.Id).HasName("pk_department_positions");
        
        builder
            .Property(dp => dp.Id)
            .HasConversion(dp => dp.Value, id => new DepartmentPositionId(id))
            .HasColumnName("department_position_id");
    }
}