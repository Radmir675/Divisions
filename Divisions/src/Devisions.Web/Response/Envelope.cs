using System.Text.Json.Serialization;
using Shared.Errors;

namespace Devisions.Web.Response;

public class Envelope
{
    public object? Result { get; }

    public Errors? Errors { get; }

    public bool IsError => Errors != null && Errors.Any();

    public DateTime? TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(object? result, Errors? failure)
    {
        Result = result;
        Errors = failure;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok(object? result = null) =>
        new Envelope(result, null);

    public static Envelope Error(Errors errors) =>
        new Envelope(null, errors);
}

public class Envelope<T>
{
    public T? Result { get; }

    public Errors? Errors { get; }

    public bool IsError => Errors != null && Errors.Any();

    public DateTime? TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(T? result, Errors? failure)
    {
        Result = result;
        Errors = failure;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope<T> Ok(T? result = default) =>
        new(result, null);

    public static Envelope<T> Error(Errors errors) =>
        new(default, errors);
}