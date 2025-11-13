namespace Devisions.Contracts.Locations.Requests;

public record AddressRequest(string Country, string City, string Street, int HouseNumber, int? RoomNumber);