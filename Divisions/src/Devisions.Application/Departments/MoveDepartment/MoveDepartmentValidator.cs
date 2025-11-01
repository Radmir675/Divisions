using Devisions.Application.Validation;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Departments.MoveDepartment;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(i => i.DepartmentId)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("Department ID"));
    }
}