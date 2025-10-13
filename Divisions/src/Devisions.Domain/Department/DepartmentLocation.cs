using System;
using Devisions.Domain.Location;

namespace Devisions.Domain.Department;

public class DepartmentLocation
{
    public Guid Id { get; private set; }

    public Department Department { get; private set; } //TODO: сюда может ID?

    public LocationId LocationId { get; private set; }

    // EF Core
    private DepartmentLocation() { }

    public DepartmentLocation(Guid id, Department department, LocationId locationId)
    {
        Id = id;

        Department = department;

        LocationId = locationId;
    }
}