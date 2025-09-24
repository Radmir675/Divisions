using Devisions.Domain.Department;
using Devisions.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devisions.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.HasKey(dp => dp.Id);

        builder.ToTable("department_positions");

        builder.Property(dp => dp.Id)
            .HasColumnName("id");

        builder.Property(dp => dp.PositionId)
            .HasColumnName("position_id");

        builder.HasOne(dp => dp.Department)
            .WithMany(dp => dp.DepartmentPositions)
            .HasForeignKey("department_id")
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne<Position>()
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}