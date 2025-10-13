namespace Devisions.Contracts.Positions;

public record PositionRequest(string Name, string? Description, Guid[] DepartmentIds);