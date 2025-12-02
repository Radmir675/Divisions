using CSharpFunctionalExtensions;
using Devisions.Application;
using Devisions.Application.Departments;
using Devisions.Application.Locations;
using Devisions.Application.Positions;
using Devisions.Application.Transaction;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.BackgroundServices;

public class DivisionCleanerService : IDivisionCleanerService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DivisionCleanerService> _logger;

    public DivisionCleanerService(
        IDepartmentRepository departmentRepository,
        IPositionsRepository positionsRepository,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<DivisionCleanerService> logger)
    {
        _departmentRepository = departmentRepository;
        _positionsRepository = positionsRepository;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> Process(CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;

        using var transactionScope = transactionScopeResult.Value;

        var departmentsRemoveResult = await DeleteDepartments(cancellationToken);
        if (departmentsRemoveResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentsRemoveResult.Error;
        }

        var locationsRemoveResult = await DeleteLocations(cancellationToken);
        if (locationsRemoveResult.IsFailure)
        {
            transactionScope.Rollback();
            return locationsRemoveResult.Error;
        }

        var positionsRemoveResult = await DeletePositions(cancellationToken);
        if (positionsRemoveResult.IsFailure)
        {
            transactionScope.Rollback();
            return positionsRemoveResult.Error;
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitResult.Error;
        }

        _logger.LogInformation("Division Cleaner service process complete.");
        return UnitResult.Success<Error>();
    }

    private async Task<UnitResult<Error>> DeletePositions(CancellationToken cancellationToken)
    {
        var positionsToDeleteResult = await _positionsRepository.GetRemovableAsync(cancellationToken);
        var positionsToDelete = positionsToDeleteResult.ToList();
        if (positionsToDelete.Count != 0)
        {
            positionsToDelete.ForEach(location => location.Lock());

            var result = await _positionsRepository.DeleteAsync(
                positionsToDelete.Select(l => l.Id).ToArray(),
                cancellationToken);
            if (result.IsFailure)
                return result.Error;

            _logger.LogInformation("Successfully deleted {count} positions", positionsToDelete.Count);
        }

        return UnitResult.Success<Error>();
    }

    private async Task<UnitResult<Error>> DeleteLocations(CancellationToken cancellationToken)
    {
        var locationsToDeleteResult = await _locationRepository.GetRemovableAsync(cancellationToken);
        var locationsToDelete = locationsToDeleteResult.ToList();
        if (locationsToDelete.Count != 0)
        {
            locationsToDelete.ForEach(location => location.Lock());

            var result = await _locationRepository.DeleteAsync(
                locationsToDelete.Select(l => l.Id).ToArray(),
                cancellationToken);
            if (result.IsFailure)
                return result.Error;

            _logger.LogInformation("Successfully deleted {count} locations", locationsToDelete.Count);
        }

        return UnitResult.Success<Error>();
    }

    private async Task<UnitResult<Error>> DeleteDepartments(CancellationToken cancellationToken)
    {
        var departments =
            await _departmentRepository.GetRemovableAsync(cancellationToken);
        var departmentsToDelete = departments.ToList();
        departmentsToDelete.ForEach(d => d.Lock());

        foreach (var department in departmentsToDelete)
        {
            var descendantIdsResult = await _departmentRepository
                .LockDescendants(department.Path, cancellationToken);
            if (descendantIdsResult.IsFailure)
                return descendantIdsResult.Error;

            // обновление родителя у департамента
            // найдем первое активное подразделение и перенесем к родителю от department
            if (descendantIdsResult.Value.Any())
            {
                var firstActiveDepartment = await
                    _departmentRepository.GetByAsync(
                        x => x.Id == descendantIdsResult.Value.First() && x.IsActive,
                        cancellationToken);
                if (firstActiveDepartment.IsFailure)
                    return firstActiveDepartment.Error;

                firstActiveDepartment.Value.UpdateParent(department.ParentId);

                var depthUpdateResult = await _departmentRepository.UpdateDescendantsDepthAsync(
                    department.Path,
                    -1,
                    cancellationToken);

                if (depthUpdateResult.IsFailure)
                    return depthUpdateResult.Error;

                var newPath = department.RemovePath();

                var pathUpdateResult = await _departmentRepository.UpdateDescendantsPathAsync(
                    department.Path,
                    newPath,
                    cancellationToken);

                if (pathUpdateResult.IsFailure)
                    return pathUpdateResult.Error;
            }
        }

        var departmentsDeleteResult = await _departmentRepository.DeleteAsync(departmentsToDelete, cancellationToken);
        if (departmentsDeleteResult.IsFailure)
            return departmentsDeleteResult.Error;

        _logger.LogInformation("Successfully deleted {count} departments", departmentsDeleteResult.Value.Count());
        return UnitResult.Success<Error>();
    }
}