using Devisions.Contracts.Locations;
using Devisions.Domain;
using Devisions.Domain.Location;
using FluentValidation;

namespace Devisions.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(x => x.Name)
            .MaximumLength(LengthConstants.LENGTH120)
            .WithMessage("Name is more than 120 characters");

        RuleFor(x => x.TimeZone)
            .NotEmpty().WithMessage("TimeZone is required")
            .Must(Timezone.IsTimeZoneValid).WithMessage("TimeZone is not valid");

        RuleFor(x => x.Address).SetValidator(new AdressValidator());
    }
}