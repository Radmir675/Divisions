using Devisions.Domain.Department;
using Devisions.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devisions.Infrastructure.Postgres.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.HasKey(dp => dp.Id);

        builder.ToTable("department_positions");

        builder.Property(dp => dp.Id)
            .HasColumnName("id");

        builder.Property(dp => dp.PositionId)
            .HasConversion(
                x => x.Value,
                guid => new PositionId(guid))
            .HasColumnName("position_id");

        builder
            .Property(x => x.DepartmentId)
            .HasConversion(
                x => x.Value,
                guid => new DepartmentId(guid))
            .HasColumnName("department_id");
    }
}