using Devisions.Application.Validation;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Departments.Commands.SoftDelete;

public class DeleteDepartmentValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));
    }
}