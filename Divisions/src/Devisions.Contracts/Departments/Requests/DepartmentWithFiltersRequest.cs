namespace Devisions.Contracts.Departments.Requests;

public record RootDepartmentsRequest(DepartmentWithFiltersRequest Request, int? Prefetch = 3);

public record DepartmentWithFiltersRequest(int? Page = 1, int? Size = 20);