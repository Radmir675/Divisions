namespace Devisions.Contracts.Departments;

public record CreateDepartmentRequest(string Name, string Identifier, Guid? ParentId, Guid[] LocationsId);