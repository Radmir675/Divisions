using System.Linq;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using Devisions.Domain.Position;

namespace Devisions.Application.Database;

public interface IReadDbContext
{
    IQueryable<Location> LocationsRead { get; }

    IQueryable<Department> DepartmentsRead { get; }

    IQueryable<Position> PositionsRead { get; }

    IQueryable<DepartmentPosition> DepartmentPositionsRead { get; }

    IQueryable<DepartmentLocation> DepartmentLocationsRead { get; }
}