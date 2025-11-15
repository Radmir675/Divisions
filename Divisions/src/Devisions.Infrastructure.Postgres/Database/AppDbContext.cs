using System.Reflection;
using Devisions.Application.Database;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Devisions.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Devisions.Infrastructure.Postgres.Database;

public sealed class AppDbContext : DbContext, IReadDbContext
{
    private readonly string _connectionString;

    public DbSet<Location> Locations { get; set; }

    public DbSet<Department> Departments { get; set; }

    public DbSet<Position> Positions { get; set; }

    public IQueryable<Location> LocationsRead => Set<Location>().AsQueryable().AsNoTracking();

    public IQueryable<Department> DepartmentsRead => Set<Department>().AsQueryable().AsNoTracking();

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);

        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseLoggerFactory(ConsoleDbLogger());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.HasPostgresExtension("ltree");
    }

    private ILoggerFactory ConsoleDbLogger() =>
        LoggerFactory.Create(builder => builder.AddConsole());
}