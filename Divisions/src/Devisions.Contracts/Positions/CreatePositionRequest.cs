namespace Devisions.Contracts.Positions;

public record CreatePositionRequest(string Name, string? Description, Guid[] DepartmentIds);