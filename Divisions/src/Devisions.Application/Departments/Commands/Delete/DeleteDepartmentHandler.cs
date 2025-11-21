using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Extensions;
using Devisions.Application.Transaction;
using Devisions.Domain.Department;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Departments.Commands.Delete;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;

public class DeleteDepartmentHandler : ICommandHandler<Guid, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
    private readonly ILogger<DeleteDepartmentHandler> _logger;

    public DeleteDepartmentHandler(
        IDepartmentRepository departmentRepository,
        ITransactionManager transactionManager,
        IValidator<SoftDeleteDepartmentCommand> validator,
        ILogger<DeleteDepartmentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
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

        if (!lockedDepartment.Value.IsActive)
        {
            return Error.NotFound("not.found", "Not found active department",
                command.DepartmentId, nameof(command.DepartmentId)).ToErrors();
        }

        lockedDepartment.Value.SoftDelete();


        var transactionResult = transactionScope.Commit();
        if (transactionResult.IsFailure)
        {
            transactionScope.Rollback();
            return transactionResult.Error.ToErrors();
        }

        return Guid.Empty;
        // _logger.LogInformation("Department is created with id: {Id}", departmentCreationResult.Value.Id);
        // return departmentCreationResult.Value.Id.Value;
    }
}