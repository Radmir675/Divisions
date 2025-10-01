using Devisions.Application.Exceptions;
using Shared.Errors;

namespace Devisions.Application.Locations.Exceptions;

public class LocationValidationException : BadRequestException
{
    public LocationValidationException(Error[] errors)
        : base(errors)
    {
    }
}