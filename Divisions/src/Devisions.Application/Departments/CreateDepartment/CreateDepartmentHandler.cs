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

namespace Devisions.Application.Departments.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentRequest Request) : ICommand;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly ILocationRepository _locationRepository;

    public CreateDepartmentHandler(
        IValidator<CreateDepartmentCommand> validator,
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ILogger<CreateDepartmentHandler> logger)
    {
        _validator = validator;
        _departmentRepository = departmentRepository;
        _logger = logger;
        _locationRepository = locationRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var departmentName = DepartmentName.Create(command.Request.Name).Value;

        var identifier = Identifier.Create(command.Request.Identifier).Value; 
        var identifierFreeCheckResult = await _departmentRepository.IsIdentifierFreeAsync(
            identifier,
            cancellationToken);

        if (identifierFreeCheckResult.IsFailure)
            return identifierFreeCheckResult.Error.ToErrors();

        if (!identifierFreeCheckResult.Value)
            return GeneralErrors.AlreadyExist("identifier").ToErrors();

        var locations = command.Request.LocationsId.ToList();
        var locationsId = locations.Select(x => new LocationId(x)).ToList();
        var locationsExistResult = IsLocationsExist(locationsId, cancellationToken);
        if (locationsExistResult.Result.IsFailure)
            return locationsExistResult.Result.Error.ToErrors();

        Result<Department, Error> departmentCreationResult;
        if (command.Request.ParentId.HasValue)
        {
            var parentId = new DepartmentId(command.Request.ParentId.Value);
            var departmentResult = await _departmentRepository.GetByIdAsync(
                parentId,
                cancellationToken);

            if (departmentResult.IsFailure)
                return departmentResult.Error.ToErrors();

            departmentCreationResult = Department.CreateChild(
                departmentName, identifier, departmentResult.Value, locationsId);
        }
        else
        {
            departmentCreationResult = Department.CreateParent(
                departmentName, identifier, locationsId);
        }

        if (departmentCreationResult.IsFailure)
            return departmentCreationResult.Error.ToErrors();

        var dbCreationResult = await _departmentRepository.AddAsync(departmentCreationResult.Value, cancellationToken);
        if (dbCreationResult.IsFailure)
            return dbCreationResult.Error.ToErrors();

        _logger.LogInformation("Department is created with id: {Id}", departmentCreationResult.Value.Id);
        return departmentCreationResult.Value.Id.Value;
    }

    private async Task<UnitResult<Error>> IsLocationsExist(
        List<LocationId> locations,
        CancellationToken cancellationToken)
    {
        var locationExistsResult = await _locationRepository.ExistsByIdsAsync(locations, cancellationToken);
        return locationExistsResult.IsFailure ? locationExistsResult.Error : Result.Success<Error>();
    }
}