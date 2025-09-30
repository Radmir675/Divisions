using CSharpFunctionalExtensions;
using Devisions.Application.Extensions;
using Devisions.Contracts;
using Devisions.Domain.Location;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Failures;

namespace Devisions.Application.Locations;

public class LocationsService : ILocationsService
{
    private readonly ILocationRepository _repository;
    private readonly ILogger<LocationsService> _logger;
    private readonly IValidator<CreateLocationDto> _validator;

    public LocationsService(ILocationRepository repository, ILogger<LocationsService> logger,
        IValidator<CreateLocationDto> validator)
    {
        _repository = repository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Failure>> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var adressResult = Address.Create(
            dto.Address.Country,
            dto.Address.City,
            dto.Address.Street,
            dto.Address.HouseNumber,
            dto.Address.RoomNumber);

        if (adressResult.IsFailure)
        {
            return adressResult.Error.ToFailure();
        }

        var timeZoneResult = Timezone.Create(dto.TimeZone);

        if (timeZoneResult.IsFailure)
        {
            return timeZoneResult.Error.ToFailure();
        }

        var locationResult = Location.Create(dto.Name, adressResult.Value!, true, timeZoneResult.Value);
        if (locationResult.IsFailure)
        {
            return locationResult.Error.ToFailure();
        }

        var resultId = await _repository.AddAsync(locationResult.Value, cancellationToken);

        if (resultId.IsFailure)
        {
            _logger.LogWarning(resultId.Error.ToString());
            return resultId.Error.ToFailure();
        }

        _logger.LogInformation("Created location with id {resultId}", resultId);
        return resultId.Value;
    }
}