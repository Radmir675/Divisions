using Devisions.Domain;
using Devisions.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devisions.Infrastructure.Postgres.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                x => x.Value,
                x => new LocationId(x))
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name");

        builder.HasIndex(i => i.Name)
            .IsUnique()
            .HasDatabaseName("UK_locations_name");

        builder.OwnsOne(ad => ad.Address, adr =>
        {
            adr.Property(x => x.Country)
                .HasColumnName("country")
                .HasMaxLength(LengthConstants.LENGTH30);

            adr.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(LengthConstants.LENGTH30);

            adr.Property(x => x.Street)
                .HasColumnName("street")
                .HasMaxLength(LengthConstants.LENGTH30);

            adr.Property(x => x.HouseNumber)
                .HasColumnName("houseNumber")
                .HasMaxLength(LengthConstants.LENGTH5);

            adr.Property(x => x.RoomNumber)
                .HasColumnName("roomNumber")
                .HasMaxLength(LengthConstants.LENGTH5)
                .IsRequired(false);

            adr.HasIndex(ind => new
                {
                    ind.Country,
                    ind.City,
                    ind.Street,
                    ind.HouseNumber,
                    ind.RoomNumber,
                })
                .IsUnique()
                .HasDatabaseName("UK_address_unique");
        });
        builder.Navigation(x => x.Address)
            .IsRequired();

        builder.ComplexProperty(n => n.Timezone, tz =>
        {
            tz.Property(x => x.IanaTimeZone)
                .HasColumnName("timezone")
                .IsRequired();
        });

        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at")
            .IsRequired(false);

        builder.Property(x => x.Version)
            .IsConcurrencyToken()
            .HasColumnName("version");

        builder.HasMany(x => x.DepartmentLocations)
            .WithOne()
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}