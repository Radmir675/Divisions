using System.Linq;

namespace Devisions.Domain.Department;

public record Path
{
    private const char SEPARATOR = '.';
    private const string DELETED_MARK = "deleted_";
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

    public static Path SetAsDeleted(string currentPath, DepartmentId? parentId)
    {
        string path;
        string[] segments = currentPath.Split(SEPARATOR);
        string updatedSegment = DELETED_MARK + segments.Last();
        if (parentId is null)
        {
            path = string.Concat(
                string.Join(SEPARATOR.ToString(), segments.Take(segments.Length - 1)),
                updatedSegment);
        }
        else
        {
            path = string.Join(
                SEPARATOR,
                string.Join(SEPARATOR.ToString(), segments.Take(segments.Length - 1)),
                updatedSegment);
        }

        return new Path(path);
    }
}