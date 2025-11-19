using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
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
            .Property(d => d.ParentId)
            .HasConversion(d => d!.Value, id => new DepartmentId(id))
            .HasColumnName("parent_id")
            .IsRequired(false);
        
        builder
            .Property(d => d.Name)
            .HasConversion(d => d.Value, name => DepartmentName.Create(name).Value)
            .HasColumnName("name")
            .HasMaxLength(Constants.LENGTH150)
            .IsRequired();

        builder
            .Property(d => d.Identifier)
            .HasConversion(d => d.Value, identifier => DepartmentIdentifier.Create(identifier).Value)
            .HasColumnName("identifier")
            .HasMaxLength(Constants.LENGTH150)
            .IsRequired();

        builder
            .Property(d => d.Path)
            .HasColumnType("ltree")
            .HasConversion(d => d.Value, path => DepartmentPath.Create(path).Value)
            .HasColumnName("path")
            .HasMaxLength(Constants.LENGTH256)
            .IsRequired();
        
        builder.HasIndex(d => d.Path).HasMethod("gist").HasDatabaseName("idx_departments_path");
        
        builder
            .Property(d => d.Depth)
            .HasColumnName("depth")
            .IsRequired();
        
        builder
            .Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        builder
            .Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder
            .Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        builder
            .Property(d => d.DeletedAt)
            .HasColumnName("deleted_at");
        
        builder
            .HasMany(d => d.DepartmentLocations)
            .WithOne()
            .HasForeignKey(dl => dl.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasMany(d => d.DepartmentPositions)
            .WithOne()
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}