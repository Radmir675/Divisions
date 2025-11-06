using System;
using Devisions.Domain.Location;

namespace Devisions.Domain.Department;

public sealed class DepartmentLocation
{
    public Guid Id { get; init; }

    public DepartmentId DepartmentId { get; init; } = null!;

    public LocationId LocationId { get; init; } = null!;

    public DepartmentLocation(Guid id, DepartmentId departmentId, LocationId locationId)
    {
        Id = id;

        DepartmentId = departmentId;

        LocationId = locationId;
    }

    public DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        DepartmentId = departmentId;

        LocationId = locationId;
    }

    // EF Core
    private DepartmentLocation() { }
}