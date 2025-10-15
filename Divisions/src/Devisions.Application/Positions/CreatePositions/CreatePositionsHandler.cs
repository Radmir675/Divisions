using System;
using System.Collections.Generic;
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

        // TODO: посмотреть как это будет работать
        Result<Description, Error> descriptionResult = default;
        if (command.Request.Description != null)
        {
            descriptionResult = Description.Create(command.Request.Description);
            if (descriptionResult.IsFailure)
                return descriptionResult.Error.ToErrors();
        }

        // бизнес-логика
        var checkDepartmentsIdAsync = await CheckDepartmentsIdAsync(
            command.Request.DepartmentIds,
            cancellationToken);
        if (checkDepartmentsIdAsync.IsFailure)
            return checkDepartmentsIdAsync.Error;

        var nameFreeResult =
            await _positionsRepository.IsNameReservedAsync(positionNameResult.Value, cancellationToken);
        if (!nameFreeResult.IsFailure)
            nameFreeResult.Error.ToErrors();

        var positionId = new PositionId(Guid.NewGuid());

        var departmentPositions = command.Request.DepartmentIds
            .Select(x => new DepartmentPosition(
                Guid.NewGuid(), new DepartmentId(x), positionId)).ToList();

        var positionResult = Position.Create(positionId, positionNameResult.Value,
            descriptionResult.IsFailure ? null : descriptionResult.Value, departmentPositions);

        var positionIdResult = await _positionsRepository.AddAsync(positionResult.Value, cancellationToken);
        if (positionIdResult.IsFailure)
            return positionIdResult.Error.ToErrors();

        _logger.LogInformation("Position created with ID:{id}", positionIdResult.Value);

        return positionIdResult.Value;
    }

    private async Task<UnitResult<Errors>> CheckDepartmentsIdAsync(
        Guid[] departmentIds,
        CancellationToken cancellationToken)
    {
        var departmentsResult = await _departmentRepository.GetAllAsync(cancellationToken);
        if (departmentsResult.IsFailure)
            return departmentsResult.Error.ToErrors();

        var departmentsIdDb = departmentsResult.Value.Select(x => x.Id.Value).ToList();
        List<Error> errors = [];
        foreach (var departmentId in departmentIds)
        {
            if (!departmentsIdDb.Contains(departmentId))
            {
                errors.Add(GeneralErrors.NotFound(departmentId, "department"));
                continue;
            }

            if (departmentsResult.Value.SingleOrDefault(x => x.Id.Value == departmentId)
                is not { IsActive: true })
            {
                errors.Add(Error.Failure(
                    "department.activity.check",
                    $"department with ID:{departmentId} is not active"));
            }
        }

        return errors.Any() ? new Errors(errors) : UnitResult.Success<Errors>();
    }
}