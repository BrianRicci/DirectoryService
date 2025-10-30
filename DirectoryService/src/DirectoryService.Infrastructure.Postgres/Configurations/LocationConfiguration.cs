using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        
        builder.HasKey(l => l.Id).HasName("pk_locations");

        builder
            .Property(l => l.Id)
            .HasConversion(l => l.Value, id => new LocationId(id))
            .HasColumnName("location_id");
        
        builder
            .Property(l => l.Name)
            .HasConversion(l => l.Value, name => LocationName.Create(name).Value)
            .HasColumnName("name")
            .HasMaxLength(Constants.LENGTH120)
            .IsRequired();
        
        builder.HasIndex(l => l.Name).IsUnique();
        
        builder.ComplexProperty(l => l.Address, ib =>
        {
            ib
                .Property(l => l.Country)
                .HasColumnName("country")
                .HasMaxLength(Constants.LENGTH32)
                .IsRequired();

            ib
                .Property(l => l.Region)
                .HasColumnName("region")
                .HasMaxLength(Constants.LENGTH32)
                .IsRequired();
            
            ib
                .Property(l => l.City)
                .HasColumnName("city")
                .HasMaxLength(Constants.LENGTH32)
                .IsRequired();
            
            ib
                .Property(l => l.Street)
                .HasColumnName("street")
                .HasMaxLength(Constants.LENGTH32)
                .IsRequired();
            
            ib
                .Property(l => l.House)
                .HasColumnName("house")
                .HasMaxLength(Constants.LENGTH32)
                .IsRequired();
        });
    
        builder
            .Property(l => l.Timezone)
            .HasConversion(l => l.Value, timezone => LocationTimezone.Create(timezone).Value)
            .HasColumnName("timezone");
        
        builder
            .Property(l => l.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
        
        builder
            .Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        
        builder
            .Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
        
        builder
            .Property(l => l.DeletedAt)
            .HasColumnName("deleted_at");
        
        builder
            .HasMany(l => l.DepartmentLocations)
            .WithOne()
            .HasForeignKey(dl => dl.LocationId);
    }
}