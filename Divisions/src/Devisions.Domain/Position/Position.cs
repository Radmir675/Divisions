using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Shared.Errors;

namespace Devisions.Domain.Position;

public record PositionId(Guid Value);

public class Position
{
    public PositionId Id { get; } = null!;

    public PositionName Name { get; private set; } = null!;

    public Description? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    private List<DepartmentPosition> _departmentPositions = [];

    // EF Core
    private Position() { }

    private Position(
        PositionId id,
        PositionName name,
        Description? description,
        bool isActive,
        List<DepartmentPosition> departmentPositions)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        _departmentPositions = departmentPositions;
    }

    public static Result<Position, Error> Create(PositionId? id, PositionName name, Description? description,
        List<DepartmentPosition> departmentPositions)
    {
        var model = new Position(
            id ?? new PositionId(Guid.NewGuid()),
            name,
            description,
            true,
            departmentPositions);

        return model;
    }
}