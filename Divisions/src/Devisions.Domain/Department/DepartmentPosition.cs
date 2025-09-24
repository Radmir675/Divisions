using System;

namespace Devisions.Domain.Department;

public class DepartmentPosition
{
    public Guid Id { get; private set; }

    public Department Department { get; private set; }

    public Guid PositionId { get; private set; }

    // EF Core
    private DepartmentPosition() { }

    public DepartmentPosition(Guid id, Department department, Guid positionId)
    {
        Id = id;
        Department = department;
        PositionId = positionId;
    }
}