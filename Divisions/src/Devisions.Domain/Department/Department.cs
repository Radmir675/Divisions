using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Devisions.Domain;

public class Department
{
    private const short DEFAULT_DEPTH = 1;

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Department? Parent { get; private set; }

    public string Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private readonly List<Department> _children = [];

    public IReadOnlyList<Department> Children => _children;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    private List<DepartmentLocation> _departmentLocations;

    private List<DepartmentPosition> _departmentPositions;

    public void Add(Department childDepartment)
    {
        childDepartment.Parent = this;
        childDepartment.Depth++;
        _children.Add(childDepartment);
        childDepartment.Path = GetPath(this, childDepartment.Path);
        UpdatedAt = DateTime.Now;
        childDepartment.UpdatedAt = UpdatedAt;
    }

    public void Rename(string name)
    {
        Name = name;
        UpdatedAt = DateTime.Now;
    }


    private Department(Guid id, string name, Identifier identifier, string path, short depth, bool isActive,
        IEnumerable<Guid> departmentLocations,
        IEnumerable<Guid> departmentPositions)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        _departmentLocations = departmentLocations
            .Select(departmentLocation => new DepartmentLocation(Guid.NewGuid(), this, departmentLocation))
            .ToList();
        _departmentPositions = departmentPositions
            .Select(departmentPosition => new DepartmentPosition(Guid.NewGuid(), this, departmentPosition))
            .ToList();
    }

    public static Result<Department, string> Create(string name, string identifier, Department parent,
        string path, bool isActive, IEnumerable<Guid> departmentLocations, IEnumerable<Guid> departmentPositions)
    {
        if (string.IsNullOrWhiteSpace(name))
            return $"Name cannot be null or whitespace.";

        if (name.Length is > 150 or < 3)
            return $"Name must be between 3 and 150 characters.";

        var identify = Identifier.Create(identifier);

        if (identify.IsFailure)
            return identify.Error;

        if (string.IsNullOrEmpty(path))
            return $"Path cannot be null or empty.";

        if (path.ToCharArray().Any(c => !char.IsLetterOrDigit(c)))
            return $"Path must contain only letters, digits and underscores.";

        var currentPath = GetPath(parent, path);

        var depth = (short)(DEFAULT_DEPTH + parent?.Depth)!;

        var department = new Department(Guid.NewGuid(), name, identify.Value, currentPath, depth, isActive,
            departmentLocations,
            departmentPositions);

        return Result.Success<Department, string>(department);
    }

    private static string GetPath(Department parent, string path) =>
        (parent?.Path != null ? parent.Path + "/" : string.Empty) + path;
}