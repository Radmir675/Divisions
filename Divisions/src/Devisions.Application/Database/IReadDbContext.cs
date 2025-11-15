using System.Linq;
using Devisions.Domain.Department;
using Devisions.Domain.Location;

namespace Devisions.Application.Database;

public interface IReadDbContext
{
    IQueryable<Location> LocationsRead { get; }

    IQueryable<Department> DepartmentsRead { get; }
}