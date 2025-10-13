using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Devisions.Domain.Location;
using Shared.Errors;

namespace Devisions.Domain.Department;

public record DepartmentId(Guid Value);

public class Department
{
    private const short DEFAULT_DEPTH = 0;

    public DepartmentId Id { get; private set; }

    public DepartmentName Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public DepartmentId? Parent { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Department> _children = [];

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    private List<DepartmentLocation> _departmentLocations;

    private List<DepartmentPosition> _departmentPositions;

    public UnitResult<Error> Rename(string name)
    {
        var nameResult = DepartmentName.Create(name);
        if (nameResult.IsFailure)
            return nameResult.Error;

        Name = nameResult.Value;
        UpdatedAt = DateTime.Now;
        return Result.Success<Error>();
    }

    // EF Core
    private Department() { }

    private Department(
        DepartmentId id,
        DepartmentName name,
        Identifier identifier,
        string path,
        short depth,
        bool isActive,
        IEnumerable<LocationId> departmentLocations,
        Department? parent = null)
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
                Guid.NewGuid(),
                this, departmentLocation))
            .ToList();
        Parent = parent?.Id;
        parent?._children.Add(this);
    }

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        Identifier identifier,
        IEnumerable<LocationId> departmentLocations)
    {
        var currentPath = GetPath(identifier.Identify);

        var depth = DEFAULT_DEPTH;

        var departmentId = new DepartmentId(Guid.NewGuid());

        var department = new Department(departmentId, name, identifier, currentPath, depth, true,
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
        var path = GetPath(identifier.Identify, parent.Path);
        var depth = parent.Depth++;

        var department = new Department(departmentId, name, identifier, path, depth, true, departmentLocations);
        return department;
    }

    private static string GetPath(string identifier, string? parentPath = null) =>
        (parentPath != null ? parentPath + "/" : string.Empty) + identifier;
}