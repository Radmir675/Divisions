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
            .HasColumnName("id")
            .HasConversion(
                x => x.Value,
                x => new DepartmentId(x));

        builder.OwnsOne(x => x.Name, n =>
        {
            n.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();
        });
        builder.Navigation(x => x.Name)
            .IsRequired();

        builder.OwnsOne(i => i.Identifier, idn =>
        {
            idn.Property(v => v.Identify)
                .IsRequired()
                .HasColumnName("identify")
                .HasMaxLength(LengthConstants.LENGTH150);
        });

        builder.Navigation(i => i.Identifier)
            .IsRequired();

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
    }
}