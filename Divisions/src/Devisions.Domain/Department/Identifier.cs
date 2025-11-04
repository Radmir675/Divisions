using System.Linq;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Domain.Department;

public record Identifier
{
    public string Identify { get; } = null!;

    private Identifier(string identifier)
    {
        Identify = identifier;
    }

    // EF Core
    private Identifier() { }

    public static Result<Identifier, Error> Create(string identifier)
    {
        if (identifier.Length is > 150 or < 3)
        {
            return Error.Validation(
                "identifier.create",
                "Identifier must be between 3 and 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Error.Validation(
                "identifier.create",
                "Identifier cannot be null or whitespace.");
        }

        if (identifier.Any(char.IsDigit))
        {
            return Error.Validation("identifier", "only latin, no numbers");
        }

        if (identifier.Any(char.IsWhiteSpace))
        {
            return Error.Validation("identifier", "instead whitespace characters use \"-\" ");
        }

        var result = new Identifier(identifier);
        return result;
    }
}