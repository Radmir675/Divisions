using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Extensions;
using Devisions.Contracts.Locations;
using Devisions.Domain.Location;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _repository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationDto> _validator;

    public CreateLocationHandler(
        ILocationRepository repository,
        IValidator<CreateLocationDto> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command.request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError(validationResult.ToErrors().ToString());
            return validationResult.ToErrors();
        }

        var adressResult = Address.Create(
            command.request.Address.Country,
            command.request.Address.City,
            command.request.Address.Street,
            command.request.Address.HouseNumber,
            command.request.Address.RoomNumber);

        if (adressResult.IsFailure)
        {
            _logger.LogError(adressResult.Error.ToString());
            return adressResult.Error.ToErrors();
        }

        var timeZoneResult = Timezone.Create(command.request.TimeZone);

        if (timeZoneResult.IsFailure)
        {
            _logger.LogError(timeZoneResult.ToString());
            return timeZoneResult.Error.ToErrors();
        }

        var locationResult = Location.Create(command.request.Name, adressResult.Value!, true, timeZoneResult.Value);
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