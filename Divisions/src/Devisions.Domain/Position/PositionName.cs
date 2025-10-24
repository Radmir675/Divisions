using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Domain.Position;

public record PositionName
{
    public string Value { get; } = null!;

    // EF Core
    private PositionName() { }

    private PositionName(string name)
    {
        Value = name;
    }

    public static Result<PositionName, Error> Create(string name)
    {
        if (string.IsNullOrEmpty(name))
            return GeneralErrors.ValueIsRequired(nameof(name));

        if (name.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH100)
        {
            return Error.Validation(
                "positionName.create",
                "Name must be between 3 and 100 characters.",
                nameof(name));
        }

        return new PositionName(name);
    }
}