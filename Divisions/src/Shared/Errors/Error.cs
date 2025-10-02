namespace Shared.Errors;

public record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }
    public string? InvalidField { get; }
    public Guid? Id { get; }

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
}