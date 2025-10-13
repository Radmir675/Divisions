using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Domain.Department;

public record DepartmentName
{
    public string Name { get; }

    // EF Core
    private DepartmentName() { }

    private DepartmentName(string name)
    {
        Name = name;
    }

    public static Result<DepartmentName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsRequired("name");

        if (name.Length is > LengthConstants.LENGTH150 or < LengthConstants.LENGTH3)
        {
            return Error.Validation(
                "department.name",
                "Name must be between 3 and 150 characters.");
        }

        return new DepartmentName(name);
    }
};