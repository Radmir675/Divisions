using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Domain.Position;

public record Description
{
    public string Value { get; } = null!;

    private Description(string description)
    {
        Value = description;
    }

    // EF Core
    private Description() { }

    public static Result<Description, Error> Create(string description)
    {
        if (string.IsNullOrEmpty(description))
            return GeneralErrors.ValueIsRequired(nameof(description));

        if (description.Length >= LengthConstants.LENGTH1000)
        {
            return Error.Validation(
                "description.create",
                "Description must be less than  1000 characters",
                nameof(description));
        }

        return new Description(description);
    }
}