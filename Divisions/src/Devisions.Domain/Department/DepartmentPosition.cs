using System;
using Devisions.Domain.Position;

namespace Devisions.Domain.Department;

public class DepartmentPosition
{
    public Guid Id { get; init; }

    public DepartmentId DepartmentId { get; init; } = null!;

    public PositionId PositionId { get; init; } = null!;

    // EF Core
    private DepartmentPosition() { }

    public DepartmentPosition(Guid id, DepartmentId departmentId, PositionId positionId)
    {
        Id = id;
        DepartmentId = departmentId;
        PositionId = positionId;
    }
}