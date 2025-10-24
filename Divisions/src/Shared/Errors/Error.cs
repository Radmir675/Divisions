namespace Shared.Errors;

public record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }
    public string? InvalidField { get; }
    public Guid? Id { get; }

    private const string SEPARATOR = "||";

    private Error(string code, string message, ErrorType errorType, string? invalidField = null, Guid? id = null)
    {
        Code = code;
        Message = message;
        ErrorType = errorType;
        InvalidField = invalidField;
        Id = id;
    }

    public static Error NotFound(string? code, string message, Guid? id) =>
        new Error(code ?? "record.not.found", message, ErrorType.NOT_FOUND, null, id);

    public static Error Validation(string? code, string message, string? invalidField = null) =>
        new Error(code ?? "record.is.invalid", message, ErrorType.VALIDATION, invalidField);

    public static Error Conflict(string? code, string message) =>
        new Error(code ?? "record.is.conflict", message, ErrorType.CONFLICT);

    public static Error Failure(string? code, string message) =>
        new Error(code ?? "failure", message, ErrorType.FAILURE);

    public Errors ToErrors() => this;

    public string Serialize()
    {
        return string.Join(SEPARATOR, Code, Message, ErrorType);
    }

    public static Error Deserialize(string serializedString)
    {
        string[] parts = serializedString.Split(SEPARATOR);
        if (parts.Length < 3)
        {
            throw new ArgumentException($"Invalid format: {serializedString}");
        }

        if (Enum.TryParse<ErrorType>(parts[2], out var errorType) == false)
        {
            throw new ArgumentException("Invalid serialized format: " + serializedString);
        }

        return new Error(parts[0], parts[1], errorType);
    }
}