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
}