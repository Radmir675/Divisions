using System;
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

public class UpdateLocationsHandler : ICommandHandler<Guid, UpdateLocationsCommand>
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

    public async Task<Result<Guid, Errors>> Handle(UpdateLocationsCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var departmentResult = await GetActiveDepartment(command, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;

        var locations = command.Request.LocationsId.Select(x => new LocationId(x)).ToList();
        var isLocationsActiveResult = await _locationRepository.AllActiveAsync(locations, cancellationToken);
        if (isLocationsActiveResult.IsFailure)
            return isLocationsActiveResult.Error;

        var departmentLocations = command.Request.LocationsId;
        var department = departmentResult.Value;

        var updateLocationsResult = department.UpdateLocations(departmentLocations);
        if (updateLocationsResult.IsFailure)
            return updateLocationsResult.Error.ToErrors();

        var updateResult = await _departmentRepository.SaveChangesAsync(cancellationToken);
        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();


        return command.DepartmentId;
    }

    private async Task<Result<Department, Errors>> GetActiveDepartment(
        UpdateLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var departmentId = new DepartmentId(command.DepartmentId);
        var departmentResult = await _departmentRepository.GetByIdWithLocationsAsync(departmentId, cancellationToken);
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