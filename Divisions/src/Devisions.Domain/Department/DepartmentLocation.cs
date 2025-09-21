using System;

namespace Devisions.Domain;

public class DepartmentLocation
{
    public Guid Id { get; private set; }

    public Department Department { get; private set; }

    public Guid LocationId { get; private set; }

    public DepartmentLocation(Guid id, Department department, Guid locationId)
    {
        Id = id;

        Department = department;

        LocationId = locationId;
    }
}