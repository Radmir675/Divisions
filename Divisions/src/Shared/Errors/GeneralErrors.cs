namespace Shared.Errors;

public static class GeneralErrors
{
    public static Error ValueIsRequired(string? name = null)
    {
        string label = name ?? "value";
        return Error.Validation("value.is.invalid", $"{label} is required");
    }
}