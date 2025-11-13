using Devisions.Application.Validation;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Locations.Queries;

public class GetLocationsValidator : AbstractValidator<GetLocationQuery>
{
    public GetLocationsValidator()
    {
        RuleFor(x => x.Request.Page)
            .NotEmpty().WithError(GeneralErrors.ValueIsInvalid("page"))
            .GreaterThan(0).WithError(Error.Validation(
                "page.validation",
                "Page must be greater than 0"));

        RuleFor(x => x.Request.PageSize)
            .NotEmpty().WithError(GeneralErrors.ValueIsInvalid("PageSize"))
            .GreaterThan(0).WithError(Error.Validation(
                "page.size.validation",
                "PageSize must be greater than 0"));
    }
}