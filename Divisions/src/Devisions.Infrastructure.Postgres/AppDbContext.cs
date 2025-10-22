using System.Reflection;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Devisions.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Devisions.Infrastructure.Postgres;

public sealed class AppDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Location> Locations { get; set; }

    public DbSet<Department> Departments { get; set; }

    public DbSet<Position> Positions { get; set; }

    private ChangeTracker ChangeTracker { get; } = null!;

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);

        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseLoggerFactory(ConsoleDBLogger());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private ILoggerFactory ConsoleDBLogger() =>
        LoggerFactory.Create(builder => builder.AddConsole());
}