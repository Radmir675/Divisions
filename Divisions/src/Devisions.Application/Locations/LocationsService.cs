using CSharpFunctionalExtensions;
using Devisions.Application.Extensions;
using Devisions.Contracts.Locations;
using Devisions.Domain.Location;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

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

    public async Task<Result<Guid, Errors>> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError(validationResult.ToErrors().ToString());
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
            _logger.LogError(adressResult.Error.ToString());
            return adressResult.Error.ToErrors();
        }

        var timeZoneResult = Timezone.Create(dto.TimeZone);

        if (timeZoneResult.IsFailure)
        {
            _logger.LogError(timeZoneResult.ToString());
            return timeZoneResult.Error.ToErrors();
        }

        var locationResult = Location.Create(dto.Name, adressResult.Value!, true, timeZoneResult.Value);
        if (locationResult.IsFailure)
        {
            _logger.LogError(locationResult.Error.ToString());
            return locationResult.Error.ToErrors();
        }

        var resultId = await _repository.AddAsync(locationResult.Value, cancellationToken);

        if (resultId.IsFailure)
        {
            _logger.LogError(resultId.Error.ToString());
            return resultId.Error.ToErrors();
        }

        _logger.LogInformation("Created location with id {resultId}", resultId);
        return resultId.Value;
    }
}