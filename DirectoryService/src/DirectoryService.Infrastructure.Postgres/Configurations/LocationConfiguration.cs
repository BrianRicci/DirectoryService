using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.ValueObjects;
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
            .HasConversion(l => l.Value, name => new LocationName(name))
            .HasColumnName("name")
            .HasMaxLength(LengthConstants.LENGTH120)
            .IsRequired();
        
        builder.ComplexProperty(l => l.Address, ib =>
        {
            ib
                .Property(l => l.Country)
                .HasColumnName("country")
                .HasMaxLength(LengthConstants.LENGTH32)
                .IsRequired();

            ib
                .Property(l => l.Region)
                .HasColumnName("region")
                .HasMaxLength(LengthConstants.LENGTH32)
                .IsRequired();
            
            ib
                .Property(l => l.City)
                .HasColumnName("city")
                .HasMaxLength(LengthConstants.LENGTH32)
                .IsRequired();
            
            ib
                .Property(l => l.Street)
                .HasColumnName("street")
                .HasMaxLength(LengthConstants.LENGTH32)
                .IsRequired();
            
            ib
                .Property(l => l.House)
                .HasColumnName("house")
                .HasMaxLength(LengthConstants.LENGTH32)
                .IsRequired();
        });
        
        builder
            .Property(l => l.Timezone)
            .HasConversion(l => l.Value, timezone => new LocationTimezone(timezone))
            .HasColumnName("timezone");
        
        builder
            .HasMany(l => l.DepartmentLocations)
            .WithOne()
            .HasForeignKey(dl => dl.LocationId);
    }
}