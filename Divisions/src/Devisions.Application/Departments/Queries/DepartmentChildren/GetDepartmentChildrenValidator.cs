using Devisions.Application.Validation;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Departments.Queries.DepartmentChildren;

public class GetDepartmentChildrenValidator : AbstractValidator<DepartmentChildrenQuery>
{
    public GetDepartmentChildrenValidator()
    {
        RuleFor(x => x.ParentId).NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("ParentId"));
    }
}