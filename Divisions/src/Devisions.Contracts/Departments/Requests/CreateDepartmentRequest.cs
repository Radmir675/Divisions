namespace Devisions.Contracts.Departments.Requests;

public record CreateDepartmentRequest(string Name, string Identifier, Guid? ParentId, Guid[] LocationsId);