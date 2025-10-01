using Devisions.Application.Exceptions;
using Shared.Errors;

namespace Devisions.Application.Locations.Exceptions;

public class LocationNotFoundException : BadRequestException
{
    protected LocationNotFoundException(Error[] errors)
        : base(errors)
    {
    }
}