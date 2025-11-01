namespace Devisions.Domain.Department;

public record Path
{
    private const char SEPARATOR = '.';
    public string PathValue { get; } = null!;

    // EF Core
    private Path() { }

    private Path(string pathValue)
    {
        PathValue = pathValue;
    }

    public static Path Create(string identifier, string? rootPath = null)
    {
        string path = (rootPath != null ? rootPath + SEPARATOR : string.Empty) + identifier;
        return new Path(path);
    }
}