using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Extensions;
using Devisions.Application.Locations;
using Devisions.Application.Positions;
using Devisions.Application.Transaction;
using Devisions.Domain.Department;
using Devisions.Domain.Position;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Departments.Commands.Delete;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;

public class DeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IPositionsRepository _positionRepository;
    
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
    private readonly ILogger<DeleteDepartmentHandler> _logger;

    public DeleteDepartmentHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IValidator<SoftDeleteDepartmentCommand> validator,
        ILogger<DeleteDepartmentHandler> logger, ILocationRepository locationRepository, IPositionsRepository positionRepository)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
        _locationRepository = locationRepository;
        _positionRepository = positionRepository;
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
            .GetByIdWithLock(new DepartmentId(command.DepartmentId), cancellationToken);
        if (lockedDepartment.IsFailure)
        {
            transactionScope.Rollback();
            return lockedDepartment.Error.ToErrors();
        }

        var department = lockedDepartment.Value;


        if (!department.IsActive)
        {
            return Error.NotFound("not.found", "Not found active department",
                command.DepartmentId, nameof(command.DepartmentId)).ToErrors();
        }

        department.SoftDelete();

        var deleteResult = await _departmentRepository.Delete(department, cancellationToken);
        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();

        // теперь надо подумать с локациями и позициями
        var positionId=department.DepartmentPositions;
        var positionInUsing = GetUnusedPositions(IEnumerable<PositionId>,cancellationToken);
        var locationInUsing = true;
        if (positionInUsing == false)
        {
            //
            _positionRepository.Delete(cancellationToken);
        }
        
        if (locationInUsing == false)
        {
            //
            _locationRepository.Delete(cancellationToken);
        }
        
        var transactionResult = transactionScope.Commit();
        if (transactionResult.IsFailure)
        {
            transactionScope.Rollback();
            return transactionResult.Error.ToErrors();
        }

        _logger.LogInformation("Department is soft deleted with id: {Id}", deleteResult.Value);
        return deleteResult.Value;
    }
}