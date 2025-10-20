using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Extensions;
using Devisions.Application.Locations;
using Devisions.Contracts.Departments;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Departments.UpdateLocations;

public record UpdateLocationsCommand(Guid DepartmentId, UpdateLocationsRequest Request) : ICommand;

public class UpdateLocationsHandler : ICommandHandler<UpdateLocationsCommand>
{
    private readonly IValidator<UpdateLocationsCommand> _validator;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<UpdateLocationsHandler> _logger;

    public UpdateLocationsHandler(
        IValidator<UpdateLocationsCommand> validator,
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ILogger<UpdateLocationsHandler> logger)
    {
        _validator = validator;
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(UpdateLocationsCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var departmentResult = await GetActiveDepartment(command, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;

        var activeLocationsResult = await GetActiveLocations(command, cancellationToken);
        if (activeLocationsResult.IsFailure)
            return activeLocationsResult.Error;

        var departmentLocations = command.Request.LocationsId;
        var updateResult = departmentResult.Value.UpdateLocations(departmentLocations);
        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var repositoryUpdateResult = await _departmentRepository.UpdateAsync(
            departmentResult.Value,
            cancellationToken);

        if (repositoryUpdateResult.IsFailure)
            return updateResult.Error.ToErrors();

        return Result.Success<Errors>();
    }

    private async Task<Result<IEnumerable<Location>, Errors>> GetActiveLocations(
        UpdateLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var locationsId = command.Request.LocationsId.Select(x => new LocationId(x)).ToList();
        var locationsResult = await _locationRepository.GetByIdsAsync(locationsId, cancellationToken);
        if (locationsResult.IsFailure)
            return locationsResult.Error.ToErrors();

        var locations = locationsResult.Value.ToList();
        if (locations.Any(x => !x.IsActive))
        {
            var errors = locations
                .Select(x => Error.Validation(
                    "locations.failure",
                    $"Active location with {x.Id.Value} is not found"))
                .ToList();
            return new Errors(errors);
        }

        return locations;
    }

    private async Task<Result<Department, Errors>> GetActiveDepartment(
        UpdateLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var departmentResult = await _departmentRepository.GetByIdAsync(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();

        bool isDepartmentActive = departmentResult.Value.IsActive;
        if (!isDepartmentActive)
        {
            return Error.Validation(
                "department.failure",
                $"Active department with {departmentResult.Value.Id} is not found").ToErrors();
        }

        return departmentResult.Value;
    }
}