using Devisions.Application.Departments.Commands.Delete;
using Devisions.Application.Validation;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Departments.Commands.Create;

public class DeleteDepartmentValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));
    }
}