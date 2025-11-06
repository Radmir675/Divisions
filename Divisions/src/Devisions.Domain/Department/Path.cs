namespace Devisions.Domain.Department;

public record Path
{
    private const char SEPARATOR = '.';
    public string PathValue { get; } = null!;

    private Path(string pathValue)
    {
        PathValue = pathValue;
    }

    // EF Core
    private Path() { }

    public static Path Create(string identifier, string? rootPath = null)
    {
        string path = (rootPath != null ? rootPath + SEPARATOR : string.Empty) + identifier;
        return new Path(path);
    }
}