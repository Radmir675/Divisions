using Devisions.Contracts;
using Devisions.Domain;
using FluentValidation;

namespace Devisions.Application.Locations;

public class AdressValidator : AbstractValidator<Adress>
{
    public AdressValidator()
    {
        RuleFor(x => x.Country)
            .NotNull().WithMessage("Country is required")
            .MaximumLength(LengthConstants.LENGTH30).WithMessage("Country must not exceed 30 characters");

        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required");

        RuleFor(x => x.City).NotNull().WithMessage("City is required");

        RuleFor(x => x.Street).NotEmpty().WithMessage("Street is required");

        RuleFor(x => x.HouseNumber).NotEmpty().WithMessage("Street is required");
    }
}