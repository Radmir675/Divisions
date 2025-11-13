using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Divisions.IntegrationTests.Share;

public class DepartmentCreator
{
    private readonly IServiceProvider _services;

    public DepartmentCreator(IServiceProvider serviceProvider)
    {
        _services = serviceProvider;
    }

    public async Task<Department> CreateAsync(
        Department? parent,
        IEnumerable<LocationId> locationId,
        string identifier = "default",
        CancellationToken cancellationToken = default)
    {
        await using var scope = _services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();


        var department = (parent == null)
            ? Department.CreateParent(
                DepartmentName.Create("department").Value,
                Identifier.Create(identifier).Value,
                locationId).Value
            : Department.CreateChild(
                DepartmentName.Create("department").Value,
                Identifier.Create(identifier).Value,
                parent,
                locationId).Value;

        await dbContext.Departments.AddAsync(department, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return department;
    }
}