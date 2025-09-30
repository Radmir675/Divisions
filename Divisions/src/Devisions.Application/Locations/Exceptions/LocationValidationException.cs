using Devisions.Application.Exceptions;
using Shared.Failures;

namespace Devisions.Application.Locations.Exceptions;

public class LocationValidationException : BadRequestException
{
    public LocationValidationException(Error[] errors)
        : base(errors)
    {
    }
}