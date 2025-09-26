using CSharpFunctionalExtensions;

namespace Devisions.Domain.Location;

public record Adress
{
    public string Country { get; private set; }
    public string City { get; private set; }
    public string Street { get; private set; }
    public int HouseNumber { get; private set; }
    public int? RoomNumber { get; private set; }

    // EF Core
    private Adress() { }

    private Adress(string country, string city, string street, int houseNumber, int? roomNumber)
    {
        Country = country;
        City = city;
        Street = street;
        HouseNumber = houseNumber;
        RoomNumber = roomNumber;
    }

    public static Result<Adress> Create(string country, string city, string street, int houseNumber, int? roomNumber)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Adress>("Country is required");
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Adress>("City is required");
        if (houseNumber <= 0)
            return Result.Failure<Adress>("House number is can not be less than 0");
        if (roomNumber != null && roomNumber <= 0)
            return Result.Failure<Adress>("Room number is can not be less than 0");

        var adress = new Adress(country, city, street, houseNumber, roomNumber);
        return Result.Success(adress);
    }
}