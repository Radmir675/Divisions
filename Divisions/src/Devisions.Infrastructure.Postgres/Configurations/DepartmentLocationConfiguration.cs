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
            .HasConversion(
                x => x.Value,
                guid => new LocationId(guid))
            .HasColumnName("location_id");

        builder
            .Property(x => x.DepartmentId)
            .HasConversion(
                x => x.Value,
                guid => new DepartmentId(guid))
            .HasColumnName("department_id");
    }
}