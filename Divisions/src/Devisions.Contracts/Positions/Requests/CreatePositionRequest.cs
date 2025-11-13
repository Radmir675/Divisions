namespace Devisions.Contracts.Positions.Requests;

public record CreatePositionRequest(string Name, string? Description, Guid[] DepartmentIds);