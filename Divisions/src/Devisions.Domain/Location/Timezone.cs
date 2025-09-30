using System;
using CSharpFunctionalExtensions;
using Shared.Failures;

namespace Devisions.Domain.Location;

public record Timezone
{
    public string IanaTimeZone { get; }

    // EF Core
    private Timezone() { }

    private Timezone(string ianaTimeZone)
    {
        IanaTimeZone = ianaTimeZone;
    }

    public static Result<Timezone, Error> Create(string ianaTimeZone)
    {
        if (string.IsNullOrWhiteSpace(ianaTimeZone))
            return Error.Validation("IanaTimeZone", "Timezone cannot be empty.");

        if (IsTimeZoneValid(ianaTimeZone))
        {
            return Error.Validation("timezone.validation", "Timezone is invalid.");
        }

        string zoneStandardName = string.Empty;
        try
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZone);
            zoneStandardName = timeZone.StandardName;
        }
        catch (TimeZoneNotFoundException exception)
        {
            return Error.NotFound("timezone.not.found", "Timezone not found.", null);
        }
        catch (InvalidTimeZoneException exception)
        {
            return Error.Validation("timezone.not.valid.parse", "Timezone is invalid.");
        }

        var validTimeZone = new Timezone(zoneStandardName);
        return validTimeZone;
    }

    public static bool IsTimeZoneValid(string ianaTimeZone)
    {
        return ianaTimeZone.Contains('/') &&
               !ianaTimeZone.StartsWith("GMT");
    }
}