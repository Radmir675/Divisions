using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Devisions.Domain.Department;
using Shared.Errors;

namespace Devisions.Domain.Location;

public record LocationId(Guid Value);

public class Location
{
    public LocationId Id { get; }

    public string Name { get; private set; } = null!;

    public Address Address { get; private set; } = null!;

    public Timezone Timezone { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    private readonly List<DepartmentLocation> _departmentLocations = [];

    // EF Core
    private Location() { }

    private Location(LocationId id, string name, Address address, bool isActive, Timezone timezone)
    {
        Id = id;
        Name = name;
        Address = address;
        IsActive = isActive;
        Timezone = timezone;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static Result<Location, Error> Create(string name, Address address, bool isActive, Timezone timezone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return GeneralErrors.ValueIsRequired("Name");
        }

        if (name.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH120)
            return Error.Validation("location.length", "Name must be between 3 and 120 characters.");

        var model = new Location(new LocationId(Guid.NewGuid()), name, address, isActive, timezone);
        return model;
    }
}