using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Extensions;
using Devisions.Domain.Location;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Locations.Commands.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _repository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationHandler(
        ILocationRepository repository,
        IValidator<CreateLocationCommand> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var address = Address.Create(
            command.Request.Address.Country,
            command.Request.Address.City,
            command.Request.Address.Street,
            command.Request.Address.HouseNumber,
            command.Request.Address.RoomNumber).Value;

        var timezone = Timezone.Create(command.Request.TimeZone).Value;

        var locationResult = Location.Create(command.Request.Name, address, true, timezone);
        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

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