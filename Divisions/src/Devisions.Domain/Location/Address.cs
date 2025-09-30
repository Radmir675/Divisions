using CSharpFunctionalExtensions;
using Shared.Failures;

namespace Devisions.Domain.Location;

public record Address
{
    public string Country { get; private set; }
    public string City { get; private set; }
    public string Street { get; private set; }
    public int HouseNumber { get; private set; }
    public int? RoomNumber { get; private set; }

    // EF Core
    private Address() { }

    private Address(string country, string city, string street, int houseNumber, int? roomNumber)
    {
        Country = country;
        City = city;
        Street = street;
        HouseNumber = houseNumber;
        RoomNumber = roomNumber;
    }

    public static Result<Address, Error> Create(string country, string city, string street, int houseNumber,
        int? roomNumber)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return Error.Validation("address.country", "Country is required",
                "country");
        }

        if (string.IsNullOrWhiteSpace(city))
            return Error.Validation("address.city", "City is required", "city");

        if (houseNumber <= 0)
        {
            return Error.Validation(
                "address.houseNumber",
                "House number is can not be less than 0", "houseNumber");
        }

        if (roomNumber != null && roomNumber <= 0)
        {
            return Error.Validation(
                "address.roomNumber",
                "Room number is can not be less than 0", "roomNumber");
        }

        var adress = new Address(country, city, street, houseNumber, roomNumber);

        return adress;
    }
}