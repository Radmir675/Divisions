namespace Devisions.Contracts.Departments.Requests;

public record DepartmentWithFiltersRequest(int? Page = 1, int? Size = 20);