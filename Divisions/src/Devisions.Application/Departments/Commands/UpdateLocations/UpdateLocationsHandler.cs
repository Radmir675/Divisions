using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Extensions;
using Devisions.Application.Locations;
using Devisions.Application.Services;
using Devisions.Application.Transaction;
using Devisions.Contracts.Departments.Requests;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Departments.Commands.UpdateLocations;

public record UpdateLocationsCommand(Guid DepartmentId, UpdateLocationsRequest Request) : ICommand;

public class UpdateLocationsHandler : ICommandHandler<Guid, UpdateLocationsCommand>
{
    private readonly IValidator<UpdateLocationsCommand> _validator;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateLocationsHandler> _logger;

    public UpdateLocationsHandler(
        IValidator<UpdateLocationsCommand> validator,
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ICacheService cache,
        ILogger<UpdateLocationsHandler> logger)
    {
        _validator = validator;
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateLocationsCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        // бизнес логика
        var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();
        using var transactionScope = transactionResult.Value;

        var departmentResult = await GetActiveDepartment(command, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error;
        }

        var locations = command.Request.LocationsId.Select(x => new LocationId(x)).ToList();
        var isLocationsActiveResult = await _locationRepository.AreAllActiveAsync(locations, cancellationToken);
        if (isLocationsActiveResult.IsFailure)
        {
            transactionScope.Rollback();
            return isLocationsActiveResult.Error;
        }

        var locationsId = command.Request.LocationsId;
        var department = departmentResult.Value;

        var updateLocationsResult = department.UpdateLocations(locationsId);

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveChangesResult.Error.ToErrors();
        }

        if (updateLocationsResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateLocationsResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }

        await UpdateCache(department, cancellationToken);
        _logger.LogInformation("Department is updated with id: {Id}", departmentResult.Value.Id);

        return command.DepartmentId;
    }

    private async Task<Result<Department, Errors>> GetActiveDepartment(
        UpdateLocationsCommand command,
        CancellationToken cancellationToken)
    {
        var departmentId = new DepartmentId(command.DepartmentId);
        var departmentResult =
            await _departmentRepository.GetByIdIncludingLocationsAsync(departmentId, cancellationToken);
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

    private async Task UpdateCache(Department department, CancellationToken cancellationToken)
    {
        string key = "departments_" + department.Id.Value;
        await _cache.RemoveAsync(key, cancellationToken);
    }
}