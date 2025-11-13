namespace Devisions.Contracts.Locations.Responses;

public sealed record LocationResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public AddressResponse Address { get; init; }

    public string Timezone { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}