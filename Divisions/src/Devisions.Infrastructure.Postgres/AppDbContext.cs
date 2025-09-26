using System.Reflection;
using Devisions.Domain.Location;
using Microsoft.EntityFrameworkCore;

namespace Devisions.Infrastructure.Postgres;

public sealed class AppDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Location> Locations { get; set; }

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}