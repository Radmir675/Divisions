using Devisions.Domain;
using Devisions.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Path = Devisions.Domain.Department.Path;

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

        builder.Property(x => x.ParentId)
            .HasConversion(
                x => x!.Value,
                guid => new DepartmentId(guid))
            .HasColumnName("parent_id")
            .IsRequired(false);

        builder.ComplexProperty(x => x.Name, n =>
        {
            n.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();
        });

        builder.Property(x => x.Identifier)
            .HasConversion(
                id => id.Identify,
                value => Identifier.Create(value).Value)
            .HasColumnName("identifier")
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH150);

        builder.HasIndex(x => x.Identifier)
            .IsUnique();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active");

        builder.Property(p => p.Path)
            .HasColumnName("path")
            .HasColumnType("ltree")
            .HasConversion(
                value => value.PathValue,
                value => Path.Create(value, null))
            .IsRequired();

        builder.HasIndex(x => x.Path)
            .HasMethod("gist")
            .HasDatabaseName("idx_departments_path");

        builder.Property(p => p.Depth)
            .HasColumnName("depth")
            .IsRequired();
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Childrens)
            .HasForeignKey(x => x.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

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