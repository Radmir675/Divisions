using System;
using Devisions.Domain.Position;

namespace Devisions.Domain.Department;

public sealed class DepartmentPosition
{
    public Guid Id { get; init; }

    public DepartmentId DepartmentId { get; init; } = null!;

    public PositionId PositionId { get; init; } = null!;

    public DepartmentPosition(Guid id, DepartmentId departmentId, PositionId positionId)
    {
        Id = id;
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    // EF Core
    private DepartmentPosition() { }
}