using System;
using CSharpFunctionalExtensions;

namespace Devisions.Domain.Location;

public record Timezone
{
    public string IanaTimeZone { get; }

    private Timezone(string ianaTimeZone)
    {
        IanaTimeZone = ianaTimeZone;
    }

    public static Result<Timezone> Create(string ianaTimeZone)
    {
        if (string.IsNullOrWhiteSpace(ianaTimeZone))
            return Result.Failure<Timezone>("Timezone cannot be empty.");

        if (IsTimeZoneValid(ianaTimeZone))
        {
            return Result.Failure<Timezone>("Timezone is invalid.");
        }

        string zoneStandardName = string.Empty;
        try
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZone);
            zoneStandardName = timeZone.StandardName;
        }
        catch (TimeZoneNotFoundException exception)
        {
            Result.Failure<Timezone>("Timezone not found.");
        }

        var validTimeZone = new Timezone(zoneStandardName);
        return Result.Success(validTimeZone);
    }

    private static bool IsTimeZoneValid(string ianaTimeZone)
    {
        return ianaTimeZone.Contains('/') &&
               !ianaTimeZone.StartsWith("GMT");
    }
}