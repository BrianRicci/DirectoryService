using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
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
        
        builder
            .Property(dl => dl.DepartmentId)
            .HasConversion(dl => dl.Value, id => new DepartmentId(id))
            .HasColumnName("department_id");
        
        builder
            .Property(dl => dl.PositionId)
            .HasConversion(dl => dl.Value, id => new PositionId(id))
            .HasColumnName("position_id");
    }
}