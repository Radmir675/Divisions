using System;
using System.Linq;
using Devisions.Application.Validation;
using Devisions.Domain.Position;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Positions.Create;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithError(GeneralErrors.ValueIsRequired("create.positions.request"));

        RuleFor(x => x.Request.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(x => x.Request.Description)
            .Must(description =>
            {
                if (description == null)
                    return true;

                var result = Description.Create(description);
                return !result.IsFailure;
            })
            .WithError(GeneralErrors.ValueIsInvalid("Description"));

        RuleFor(x => x.Request.DepartmentIds)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("DepartmentIds"))
            .Must(HaveUniqueId).WithError(Error.Validation(
                "validation.departmentIds.id",
                "departmentIds is not unique",
                "DepartmentIds"));
    }

    private bool HaveUniqueId(Guid[] departmentsId)
    {
        return departmentsId.Distinct().Count() == departmentsId.Count();
    }
}