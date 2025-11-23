using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Devisions.Domain.Interfaces;
using Shared.Errors;

namespace Devisions.Domain.Position;

public record PositionId(Guid Value);

public sealed class Position:ISoftDeletable
{
    public PositionId Id { get; } = null!;

    public PositionName Name { get; private set; } = null!;

    public Description? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    private List<DepartmentPosition> _departmentPositions = [];

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

    // EF Core
    private Position() { }

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

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }

    public void Restore()
    {
        DeletedAt = null;
        IsActive = true;
    }
}