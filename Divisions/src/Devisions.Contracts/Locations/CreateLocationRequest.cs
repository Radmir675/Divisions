namespace Devisions.Contracts.Locations;

public record CreateLocationRequest(string Name, AddressRequest Address, string TimeZone);