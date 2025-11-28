using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Devisions.Domain.Interfaces;
using Devisions.Domain.Location;
using Shared.Errors;

namespace Devisions.Domain.Department;

public record DepartmentId(Guid Value);

public sealed class Department : ISoftDeletable
{
    private const short DEFAULT_DEPTH = 0;

    public DepartmentId Id { get; } = null!;

    public DepartmentName Name { get; private set; } = null!;

    public Identifier Identifier { get; private set; } = null!;

    public DepartmentId? ParentId { get; private set; }

    public Path Path { get; private set; } = null!;

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public Department? Parent { get; private set; }

    private readonly List<Department> _childrens = [];

    public IReadOnlyList<Department> Childrens => _childrens;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    private List<DepartmentLocation> _departmentLocations = [];

    private List<DepartmentPosition> _departmentPositions = [];

    public UnitResult<Error> Rename(string name)
    {
        var nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success<Error>();
    }

    // EF Core
    private Department() { }

    private Department(
        DepartmentId id,
        DepartmentName name,
        Identifier identifier,
        Path path,
        short depth,
        bool isActive,
        IEnumerable<LocationId> departmentLocations,
        DepartmentId? parentId = null)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        _departmentLocations = departmentLocations
            .Select(departmentLocation => new DepartmentLocation(
                Guid.NewGuid(), id, departmentLocation))
            .ToList();
        ParentId = parentId;
    }

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        Identifier identifier,
        IEnumerable<LocationId> departmentLocations)
    {
        var currentPath = Path.Create(identifier.Identify);

        var departmentId = new DepartmentId(Guid.NewGuid());

        var department = new Department(departmentId, name, identifier, currentPath, DEFAULT_DEPTH, true,
            departmentLocations);

        return department;
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        Identifier identifier,
        Department parent,
        IEnumerable<LocationId> departmentLocations)
    {
        var departmentId = new DepartmentId(Guid.NewGuid());
        var path = Path.Create(identifier.Identify, parent.Path.PathValue);
        short depth = (short)(parent.Depth + 1);

        var department = new Department(
            departmentId, name, identifier, path, depth, true, departmentLocations, parent.Id);

        return department;
    }

    public UnitResult<Error> UpdateLocations(Guid[] locationIds)
    {
        if (locationIds.Length == 0)
            return GeneralErrors.ValueIsRequired("departmentLocations");

        var newDepartmentLocations = locationIds.Select(d =>
            new DepartmentLocation(Id, new LocationId(d)));

        _departmentLocations = newDepartmentLocations.ToList();

        return Result.Success<Error>();
    }

    public UnitResult<Error> MoveTo(Department? parent)
    {
        if (parent is null)
        {
            Path = Path.Create(Identifier.Identify);
            Depth = DEFAULT_DEPTH;
            ParentId = null;
        }
        else
        {
            Path = Path.Create(Identifier.Identify, parent.Path.PathValue);
            Depth = (short)(parent.Depth + 1);
            ParentId = parent.Id;
        }

        return UnitResult.Success<Error>();
    }

    public void SoftDelete()
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        Path = Path.SetAsDeleted(Path.PathValue, ParentId);
    }

    public void Restore()
    {
        IsActive = true;
        DeletedAt = null;
    }
}