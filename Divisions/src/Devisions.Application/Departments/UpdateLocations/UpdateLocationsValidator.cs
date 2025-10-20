using Devisions.Application.Validation;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Departments.UpdateLocations;

public class UpdateLocationsValidator : AbstractValidator<UpdateLocationsCommand>
{
    public UpdateLocationsValidator()
    {
        RuleFor(x => x.Request.LocationsId)
            .NotNull().WithError(GeneralErrors.ValueIsRequired("LocationsId"))
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("LocationsId"))
            .MustHaveUniqueValues();
    }
}