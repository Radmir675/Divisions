using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Extensions;
using Devisions.Application.Services;
using Devisions.Application.Transaction;
using Devisions.Domain.Department;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Departments.Commands.Move;

public record MoveDepartmentCommand(Guid DepartmentId, Guid? ParentId) : ICommand;

public class MoveDepartmentHandler : ICommandHandler<Guid, MoveDepartmentCommand>
{
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ICacheService _cache;
    private readonly ITransactionManager _transactionManager;

    public MoveDepartmentHandler(
        IValidator<MoveDepartmentCommand> validator,
        IDepartmentRepository departmentRepository,
        ICacheService cache,
        ITransactionManager transactionManager)
    {
        _validator = validator;
        _departmentRepository = departmentRepository;
        _cache = cache;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(MoveDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        // бизнес логика
        var departmentId = new DepartmentId(command.DepartmentId);

        Department parent = null!;
        if (command.ParentId.HasValue)
        {
            var parentId = new DepartmentId(command.ParentId.Value);

            if (parentId == departmentId)
            {
                transactionScope.Rollback();
                return Error.Validation(
                        "input.failure",
                        "Parent id and department id are the same")
                    .ToErrors();
            }

            // поиск и блокировка родительского подразделения
            var parentResult = await _departmentRepository.GetByIdWithLock(parentId, cancellationToken);
            if (parentResult.IsFailure)
            {
                transactionScope.Rollback();
                return parentResult.Error.ToErrors();
            }

            parent = parentResult.Value;
        }

        // поиск и блокировка основного подразделения
        var departmentResult = await _departmentRepository.GetByIdWithLock(departmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        // поиск и блокировка дочерних подразделений
        var lockDescendanceResult = await
            _departmentRepository.LockDescendants(department.Path, cancellationToken);
        if (lockDescendanceResult.IsFailure)
        {
            transactionScope.Rollback();
            return lockDescendanceResult.Error.ToErrors();
        }

        var descendanceIds = lockDescendanceResult.Value;

        if (parent != null && descendanceIds.Contains(parent.Id))
        {
            transactionScope.Rollback();
            return Error
                .Conflict("parent.id.failure", "ParentId is the same with children Id")
                .ToErrors();
        }

        var oldDepartmentPath = department.Path;
        var oldDepartmentDepth = department.Depth;

        // обновление родителя у департамента
        var moveToParentResult = department.MoveTo(parent);
        if (moveToParentResult.IsFailure)
        {
            transactionScope.Rollback();
            return moveToParentResult.Error.ToErrors();
        }

        var savingResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (savingResult.IsFailure)
        {
            transactionScope.Rollback();
            return savingResult.Error.ToErrors();
        }

        int deltaDepth = GetDeltaDepartmentDepth(department.Depth, oldDepartmentDepth);

        var depthUpdateResult =
            await _departmentRepository.UpdateDescendantsDepthAsync(oldDepartmentPath, deltaDepth, cancellationToken);
        if (depthUpdateResult.IsFailure)
        {
            transactionScope.Rollback();
            return depthUpdateResult.Error.ToErrors();
        }

        var pathUpdateResult =
            await _departmentRepository.UpdateDescendantsPathAsync(
                oldDepartmentPath,
                department.Path,
                cancellationToken);

        if (pathUpdateResult.IsFailure)
        {
            transactionScope.Rollback();
            return pathUpdateResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitResult.Error.ToErrors();
        }

        await UpdateCache(cancellationToken);

        return command.DepartmentId;
    }

    private static int GetDeltaDepartmentDepth(short newDepth, short oldDepth) =>
        newDepth - oldDepth;

    private async Task UpdateCache(CancellationToken cancellationToken)
    {
        const string key = "departments_";
        await _cache.RemoveByPrefixAsync(key, cancellationToken);
    }
}