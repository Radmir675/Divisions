using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Database;
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
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        IValidator<CreateDepartmentCommand> validator,
        IDepartmentRepository departmentRepository,
        ILocationRepository locationRepository,
        ITransactionManager transactionManager,
        ILogger<CreateDepartmentHandler> logger)
    {
        _validator = validator;
        _departmentRepository = departmentRepository;
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        var departmentName = DepartmentName.Create(command.Request.Name).Value;

        var identifier = Identifier.Create(command.Request.Identifier).Value;


        // бизнес логика
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var identifierExistsResult = await _departmentRepository.IsIdentifierAlreadyExistsAsync(
            identifier,
            cancellationToken);

        if (identifierExistsResult.IsFailure)
        {
            transactionScope.Rollback();
            return identifierExistsResult.Error.ToErrors();
        }

        if (identifierExistsResult.Value)
        {
            transactionScope.Rollback();
            return GeneralErrors.AlreadyExist("identifier").ToErrors();
        }

        var locations = command.Request.LocationsId.ToList();
        var locationsId = locations.Select(x => new LocationId(x)).ToList();
        var locationsExistResult = await _locationRepository.ExistsByIdsAsync(locationsId, cancellationToken);
        if (locationsExistResult.IsFailure)
        {
            transactionScope.Rollback();
            return locationsExistResult.Error;
        }

        Result<Department, Error> departmentCreationResult;
        if (command.Request.ParentId.HasValue)
        {
            var parentId = new DepartmentId(command.Request.ParentId.Value);
            var departmentResult = await _departmentRepository.GetByIdAsync(
                parentId,
                cancellationToken);

            if (departmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return departmentResult.Error.ToErrors();
            }

            departmentCreationResult = Department.CreateChild(
                departmentName, identifier, departmentResult.Value, locationsId);
        }
        else
        {
            departmentCreationResult = Department.CreateParent(
                departmentName, identifier, locationsId);
        }

        if (departmentCreationResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentCreationResult.Error.ToErrors();
        }

        var dbCreationResult = await _departmentRepository.AddAsync(departmentCreationResult.Value, cancellationToken);
        if (dbCreationResult.IsFailure)
        {
            transactionScope.Rollback();
            return dbCreationResult.Error.ToErrors();
        }

        var transactionResult = transactionScope.Commit();
        if (transactionResult.IsFailure)
            return transactionResult.Error.ToErrors();

        _logger.LogInformation("Department is created with id: {Id}", departmentCreationResult.Value.Id);
        return departmentCreationResult.Value.Id.Value;
    }
}