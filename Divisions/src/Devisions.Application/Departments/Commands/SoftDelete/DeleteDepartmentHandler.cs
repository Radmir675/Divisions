using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Database;
using Devisions.Application.Extensions;
using Devisions.Application.Locations;
using Devisions.Application.Positions;
using Devisions.Application.Transaction;
using Devisions.Domain.Department;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Departments.Commands.SoftDelete;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;

public class SoftDeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IPositionsRepository _positionRepository;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;

    public SoftDeleteDepartmentHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IValidator<SoftDeleteDepartmentCommand> validator,
        ILogger<SoftDeleteDepartmentHandler> logger,
        ILocationRepository locationRepository,
        IPositionsRepository positionRepository,
        IDbConnectionFactory dbConnectionFactory)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
        _locationRepository = locationRepository;
        _positionRepository = positionRepository;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<Guid, Errors>> Handle(
        SoftDeleteDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        // бизнес логика
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var lockedDepartment = await _departmentRepository
            .GetByIdWithLock(
                new DepartmentId(command.DepartmentId),
                cancellationToken);
        if (lockedDepartment.IsFailure)
        {
            transactionScope.Rollback();
            return lockedDepartment.Error.ToErrors();
        }

        var department = lockedDepartment.Value;

        if (!department.IsActive)
        {
            transactionScope.Rollback();
            return Error.NotFound("not.found", "Not found active department",
                command.DepartmentId, nameof(command.DepartmentId)).ToErrors();
        }

        var descendantsIdResult = await _departmentRepository.LockDescendants(department.Path, cancellationToken);
        if (descendantsIdResult.IsFailure)
        {
            transactionScope.Rollback();
            return descendantsIdResult.Error.ToErrors();
        }

        var oldPath = department.Path;
        department.SoftDelete();
        var newPath = department.Path;

        var descendantsId = descendantsIdResult.Value.Select(id => id.Value).ToList();
        if (descendantsId.Count > 0)
        {
            var updatePathDescendants =
                await _departmentRepository.UpdateDescendantsPathAsync(oldPath, newPath, cancellationToken);
            if (updatePathDescendants.IsFailure)
            {
                transactionScope.Rollback();
                return updatePathDescendants.Error.ToErrors();
            }
        }

        _logger.LogInformation("Descendants path updated {descendants}", string.Join("; ", descendantsId));

        // проверка позиций на активность
        var unusedPositionsResult =
            await _positionRepository.GetPositionsExclusiveToAsync(department.Id, cancellationToken);
        if (unusedPositionsResult.IsFailure)
        {
            transactionScope.Rollback();
            return unusedPositionsResult.Error.ToErrors();
        }

        if (unusedPositionsResult.Value.Any())
        {
            var positionsResult = await _positionRepository
                .GetByIdsAsync(unusedPositionsResult.Value, cancellationToken);
            if (positionsResult.IsFailure)
            {
                transactionScope.Rollback();
                return positionsResult.Error.ToErrors();
            }

            var positions = positionsResult.Value.ToList();
            positions.ForEach(x => x.SoftDelete());

            string deletedPositions = string.Join("; ", positions.Select(x => x.Id.Value));
            _logger.LogInformation("These positions are soft deleted:{locations}", deletedPositions);
        }

        // проверка локаций на активность
        var unusedLocationsResult =
            await _locationRepository.GetExclusiveToDepartmentAsync(department.Id, cancellationToken);
        if (unusedLocationsResult.IsFailure)
        {
            transactionScope.Rollback();
            return unusedLocationsResult.Error.ToErrors();
        }

        if (unusedLocationsResult.Value.Any())
        {
            var locationsResult = await _locationRepository
                .GetByIdsAsync(unusedLocationsResult.Value, cancellationToken);
            if (locationsResult.IsFailure)
            {
                transactionScope.Rollback();
                return locationsResult.Error.ToErrors();
            }

            var locations = locationsResult.Value.ToList();
            locations.ForEach(x => x.SoftDelete());
            string deletedLocations = string.Join("; ", locations.Select(x => x.Id.Value));
            _logger.LogInformation("These locations are soft deleted:{locations}", deletedLocations);
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveChangesResult.Error.ToErrors();
        }

        var transactionResult = transactionScope.Commit();
        if (transactionResult.IsFailure)
        {
            transactionScope.Rollback();
            return transactionResult.Error.ToErrors();
        }

        _logger.LogInformation("Department is soft deleted with id: {Id}", department.Id);
        return department.Id.Value;
    }
}