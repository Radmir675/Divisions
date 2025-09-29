using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devisions.Infrastructure.Postgres.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.ToTable("department_locations");

        builder
            .Property(x => x.LocationId)
            .HasColumnName("location_id");

        builder.HasOne(dp => dp.Department)
            .WithMany(dp => dp.DepartmentLocations)
            .HasForeignKey("department_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}