using CSharpFunctionalExtensions;

namespace Devisions.Domain.Department;

public record Identifier
{
    public string Identify { get; }

    // EF Core
    private Identifier() { }

    private Identifier(string identifier)
    {
        Identify = identifier;
    }

    public static Result<Identifier, string> Create(string identifier)
    {
        if (identifier.Length is > 150 or < 3)
            return $"Identifier must be between 3 and 150 characters.";

        if (string.IsNullOrWhiteSpace(identifier))
            return $"Identifier cannot be null or whitespace.";

        var result = new Identifier(identifier);
        return Result.Success<Identifier, string>(result);
    }
}