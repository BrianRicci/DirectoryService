using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        
        builder.HasKey(p => p.Id).HasName("pk_positions");

        builder
            .Property(p => p.Id)
            .HasConversion(p => p.Value, id => new PositionId(id))
            .HasColumnName("position_id");
        
        builder
            .Property(p => p.Name)
            .HasConversion(p => p.Value, name => PositionName.Create(name).Value)
            .HasColumnName("name")
            .HasMaxLength(LengthConstants.LENGTH100)
            .IsRequired();

        builder
            .Property(p => p.Description)
            .HasConversion(p => p.Value, description => PositionDescription.Create(description).Value)
            .HasColumnName("description")
            .HasMaxLength(LengthConstants.LENGTH1000);
        
        builder
            .HasMany(p => p.DepartmentPositions)
            .WithOne()
            .HasForeignKey(dp => dp.PositionId);
    }
}