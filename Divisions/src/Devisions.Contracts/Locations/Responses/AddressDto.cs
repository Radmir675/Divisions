namespace Devisions.Contracts.Locations.Responses;

public record AddressDto
{
    public required string Country { get; init; }

    public required string City { get; init; }

    public required string Street { get; init; }

    public required int HouseNumber { get; init; }

    public int? RoomNumber { get; init; }
}