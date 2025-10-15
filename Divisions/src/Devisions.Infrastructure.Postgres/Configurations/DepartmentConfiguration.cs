using Devisions.Domain;
using Devisions.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devisions.Infrastructure.Postgres.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                x => x.Value,
                guid => new DepartmentId(guid))
            .HasColumnName("id");

        builder.ComplexProperty(x => x.Name, n =>
        {
            n.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();
        });

        builder.ComplexProperty(i => i.Identifier, idn =>
        {
            idn.Property(v => v.Identify)
                .IsRequired()
                .HasColumnName("identify")
                .HasMaxLength(LengthConstants.LENGTH150);
        });

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(p => p.Path)
            .HasColumnName("path")
            .IsRequired();

        builder.Property(p => p.Depth)
            .HasColumnName("depth")
            .IsRequired();
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(i => i.Parent)
            .WithMany()
            .HasForeignKey("parent_id")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(x => x.DepartmentLocations)
            .WithOne()
            .HasForeignKey(x => x.DepartmentId);

        builder
            .HasMany(x => x.DepartmentPositions)
            .WithOne()
            .HasForeignKey(x => x.DepartmentId);
    }
}