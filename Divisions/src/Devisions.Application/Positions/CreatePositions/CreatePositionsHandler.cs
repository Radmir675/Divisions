using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Departments;
using Devisions.Application.Extensions;
using Devisions.Domain.Department;
using Devisions.Domain.Position;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Positions.CreatePositions;

public class CreatePositionsHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<CreatePositionsHandler> _logger;

    public CreatePositionsHandler(
        IValidator<CreatePositionCommand> validator,
        IPositionsRepository positionsRepository,
        IDepartmentRepository departmentRepository,
        ILogger<CreatePositionsHandler> logger)
    {
        _validator = validator;
        _positionsRepository = positionsRepository;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var inputValidationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!inputValidationResult.IsValid)
            return inputValidationResult.ToErrors();

        var positionNameResult = PositionName.Create(command.Request.Name);
        if (positionNameResult.IsFailure)
            return positionNameResult.Error.ToErrors();

        var descriptionResult = command.Request.Description == null
            ? default
            : Description.Create(command.Request.Description);
        if (descriptionResult.IsFailure)
            return descriptionResult.Error.ToErrors();

        // бизнес-логика
        var departmentsId = command.Request.DepartmentIds
            .Select(x => new DepartmentId(x))
            .ToList();
        var checkDepartmentsIdAsync = await _departmentRepository.AllExistAndActiveAsync(
            departmentsId,
            cancellationToken);
        if (checkDepartmentsIdAsync.IsFailure)
            return checkDepartmentsIdAsync.Error;

        var nameFreeResult =
            await _positionsRepository.IsNameActiveAndFreeAsync(positionNameResult.Value, cancellationToken);
        if (nameFreeResult.IsFailure)
            nameFreeResult.Error.ToErrors();

        if (!nameFreeResult.Value)
            return Error.Conflict("active.name", "Name is engaged").ToErrors();

        var positionId = new PositionId(Guid.NewGuid());

        var departmentPositions = command.Request.DepartmentIds
            .Select(x => new DepartmentPosition(
                Guid.NewGuid(), new DepartmentId(x), positionId)).ToList();

        var createPositionResult = Position.Create(positionId, positionNameResult.Value,
            descriptionResult.IsFailure ? null : descriptionResult.Value, departmentPositions);

        var positionIdResult = await _positionsRepository.AddAsync(createPositionResult.Value, cancellationToken);
        if (positionIdResult.IsFailure)
            return positionIdResult.Error.ToErrors();

        _logger.LogInformation("Position created with ID:{id}", positionIdResult.Value);

        return positionIdResult.Value;
    }
}