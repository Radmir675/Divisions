using Devisions.Application.Validation;
using Devisions.Domain;
using Devisions.Domain.Location;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Locations.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(x => x.Request.Name)
            .MaximumLength(LengthConstants.LENGTH120)
            .WithMessage("Name is more than 120 characters");

        RuleFor(x => x.Request.TimeZone)
            .MustBeValueObject(Timezone.Create);

        RuleFor(x => x.Request.Address).SetValidator(new AdressValidator());

        RuleFor(x => x.Request.Address)
            .MustBeValueObject(c => Address
                .Create(c.Country, c.City, c.Street, c.HouseNumber, c.RoomNumber));
    }
}