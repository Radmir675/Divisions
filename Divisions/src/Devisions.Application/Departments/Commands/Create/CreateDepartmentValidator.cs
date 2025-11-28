using Devisions.Application.Validation;
using Devisions.Domain.Department;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Departments.Commands.Create;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(x => x.Request.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(i => i.Request.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(l => l.Request.LocationsId)
            .NotNull().WithError(GeneralErrors.ValueIsRequired("locationsId"))
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("locationsId"));
    }
}