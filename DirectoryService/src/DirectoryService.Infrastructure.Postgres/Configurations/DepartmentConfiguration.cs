using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        
        builder.HasKey(d => d.Id).HasName("pk_department");
        
        builder
            .Property(d => d.Id)
            .HasConversion(d => d.Value, id => new DepartmentId(id))
            .HasColumnName("department_id");
        
        builder
            .Property(d => d.Name)
            .HasConversion(d => d.Value, name => new DepartmentName(name))
            .HasColumnName("name")
            .HasMaxLength(LengthConstants.LENGTH150)
            .IsRequired();

        builder
            .Property(d => d.Identifier)
            .HasConversion(d => d.Value, identifier => new DepartmentIdentifier(identifier))
            .HasColumnName("identifier")
            .HasMaxLength(LengthConstants.LENGTH150)
            .IsRequired();

        builder
            .Property(d => d.Path)
            .HasConversion(d => d.Value, path => new DepartmentPath(path))
            .HasColumnName("path")
            .HasMaxLength(LengthConstants.LENGTH256)
            .IsRequired();
        
        builder
            .HasMany(d => d.DepartmentLocations)
            .WithOne()
            .HasForeignKey(dl => dl.DepartmentId);
        
        builder
            .HasMany(d => d.DepartmentPositions)
            .WithOne()
            .HasForeignKey(dp => dp.DepartmentId);
    }
}