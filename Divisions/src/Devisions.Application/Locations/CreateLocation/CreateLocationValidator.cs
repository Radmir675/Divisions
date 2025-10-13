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
            .WithError(GeneralErrors.ValueIsRequired("Name is required"));

        RuleFor(x => x.Request.Name)
            .MaximumLength(LengthConstants.LENGTH120)
            .WithError(Error.Validation(
                "request.name",
                "Name is more than 120 characters",
                "name"));

        RuleFor(x => x.Request.TimeZone)
            .MustBeValueObject(Timezone.Create);

        RuleFor(x => x.Request.Address)
            .MustBeValueObject(c => Address
                .Create(c.Country, c.City, c.Street, c.HouseNumber, c.RoomNumber));
    }
}