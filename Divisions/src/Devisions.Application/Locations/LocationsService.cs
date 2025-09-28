using Devisions.Contracts;
using Devisions.Domain.Location;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Adress = Devisions.Domain.Location.Adress;

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

    public async Task<Guid> CreateAsync(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var adress = Adress.Create(
            dto.Address.Country,
            dto.Address.City,
            dto.Address.Street,
            dto.Address.HouseNumber,
            dto.Address.RoomNumber);

        if (adress.IsFailure)
        {
            _logger.LogError(adress.Error);
            throw new Exception(adress.Error);
        }

        var timeZone = Timezone.Create(dto.TimeZone);

        if (timeZone.IsFailure)
        {
            _logger.LogWarning(timeZone.Error);
            throw new Exception(timeZone.Error);
        }

        var location = Location.Create(dto.Name, adress.Value!, true, timeZone.Value);
        if (location.IsFailure)
        {
            _logger.LogWarning(location.Error);
            throw new Exception(location.Error);
        }

        var resultId = await _repository.AddAsync(location.Value, cancellationToken);

        if (resultId.IsFailure)
        {
            _logger.LogWarning(resultId.Error);
            throw new Exception(resultId.Error);
        }

        _logger.LogInformation("Created location with id {resultId}", resultId);
        return resultId.Value;
    }
}