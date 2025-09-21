using System;
using CSharpFunctionalExtensions;

namespace Devisions.Domain;

public class Position
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Position(Guid id, string name, string? description, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static Result<Position, string> Create(string name, string? description)
    {
        if (name.Length is < 3 or > 100)
            return $"Name must be between 3 and 100 characters.";

        if ((description?.Length ?? 0) > 1000)
            return $"Description must be less than  1000 characters.";

        var model = new Position(Guid.NewGuid(), name, description, true);
        return Result.Success<Position, string>(model);
    }
}