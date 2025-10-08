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
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name");

        builder.HasIndex(i => i.Name)
            .IsUnique();

        builder.HasIndex(i => i.Address)
            .IsUnique();

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
                .HasMaxLength(LengthConstants.LENGTH5);
        });
        builder.Navigation(x => x.Address)
            .IsRequired();

        builder.OwnsOne(n => n.Timezone, tz =>
        {
            tz.Property(x => x.IanaTimeZone)
                .HasColumnName("timezone")
                .IsRequired();
        });
        builder.Navigation(x => x.Timezone)
            .IsRequired(false);

        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);
    }
}