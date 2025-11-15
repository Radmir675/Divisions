namespace Devisions.Contracts.Locations.Requests;

public record CreateLocationRequest(string Name, AddressRequest Address, string TimeZone);