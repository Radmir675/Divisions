using System;
using CSharpFunctionalExtensions;

namespace Devisions.Domain.Location;

public class Location
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Address { get; private set; }

    public Timezone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    private Location(Guid id, string name, string adress, bool isActive, Timezone timezone)
    {
        Id = id;
        Name = name;
        Address = adress;
        IsActive = isActive;
        Timezone = timezone;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static Result<Location, string> Create(string name, string? adress, bool isActive, Timezone timezone)
    {
        if (name.Length is < 3 or > 120)
            return $"Name must be between 3 and 120 characters.";

        if ((adress?.Length ?? 0) > 1000)
            return $"Description must be less than  1000 characters.";

        var model = new Location(new Guid(), name, adress!, isActive, timezone);
        return Result.Success<Location, string>(model);
    }
}