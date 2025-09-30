using Devisions.Application.Exceptions;
using Shared.Failures;

namespace Devisions.Application.Locations;

public class LocationNotFoundException : BadRequestException
{
    protected LocationNotFoundException(Error[] errors)
        : base(errors)
    {
    }
}