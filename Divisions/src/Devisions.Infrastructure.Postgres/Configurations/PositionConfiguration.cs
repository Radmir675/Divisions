using Devisions.Domain;
using Devisions.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devisions.Infrastructure.Postgres.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                p => p.Value,
                p => new PositionId(p))
            .HasColumnName("id");

        builder.ToTable("positions");

        builder.ComplexProperty(x => x.Name, nb =>
        {
            nb.Property(x => x.Value)
                .HasColumnName("name")
                .IsRequired();
        });

        builder.OwnsOne(x => x.Description, nb =>
        {
            nb.Property(x => x.Value)
                .HasColumnName("description")
                .HasMaxLength(LengthConstants.LENGTH150)
                .IsRequired();
        });

        builder.Navigation(n => n.Description)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        builder.HasMany(x => x.DepartmentPositions)
            .WithOne()
            .HasForeignKey(x => x.PositionId);
    }
}