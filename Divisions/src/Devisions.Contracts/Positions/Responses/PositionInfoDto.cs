namespace Devisions.Contracts.Positions.Responses;

public record PositionInfoDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;
}