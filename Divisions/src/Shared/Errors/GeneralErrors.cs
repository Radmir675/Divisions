namespace Shared.Errors;

public static class GeneralErrors
{
    public static Error ValueIsRequired(string? name = null)
    {
        string label = name ?? "value";
        return Error.Validation("value.is.invalid", $"{label} is required");
    }

    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "value";
        return Error.Validation("value.is.invalid", $"{label} is invalid");
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" by Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "record"} not found{forId}", null);
    }

    public static Error AlreadyExist(string? label = null)
    {
        return Error.Validation("record.already.exist", $"{label ?? "record"} is exists");
    }

    public static Error NonUniqueValues(string? value = null)
    {
        return Error.Validation("identical.values", "Collection exists identical values", value);
    }

    public static Error CanceledOperation(string? message = null)
    {
        return Error.Failure("operation.canceled", message ?? "Operation canceled");
    }

    public static Error DatabaseError(string? message = null)
    {
        return Error.Failure("database.error", message ?? "Database error");
    }

    public static Error NotFoundInDatabase(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" by Id '{id}'";
        return Error.NotFound(
            "record.not.found.in.database",
            $"{name ?? "record"} not found{forId} in database", null);
    }
}