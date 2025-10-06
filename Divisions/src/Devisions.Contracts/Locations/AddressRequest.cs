namespace Devisions.Contracts.Locations;

public record AddressRequest(string Country, string City, string Street, int HouseNumber, int? RoomNumber);