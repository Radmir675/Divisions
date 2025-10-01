using Devisions.Web.Response;
using Shared.Errors;

namespace Devisions.Web.EndPointResults;

public class ErrorsResult : IResult
{
    private readonly Errors _errors;


    public ErrorsResult(Error error)
    {
        _errors = error;
    }

    public ErrorsResult(Errors errors)
    {
        _errors = errors;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!_errors.Any())
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return httpContext.Response.WriteAsJsonAsync(Envelope.Error(_errors));
        }


        var distinctErrors = _errors
            .Select(x => x.ErrorType)
            .Distinct()
            .ToList();

        int statusCode = distinctErrors.Count() > 1
            ? StatusCodes.Status500InternalServerError
            : GetStatusCodeFromErrorType(distinctErrors[0]);

        var envelope = Envelope.Error(_errors);

        httpContext.Response.StatusCode = statusCode;

        return httpContext.Response.WriteAsJsonAsync(envelope);
    }

    private static int GetStatusCodeFromErrorType(ErrorType errorType) => errorType switch
    {
        ErrorType.NOT_FOUND => StatusCodes.Status404NotFound,
        ErrorType.CONFLICT => StatusCodes.Status409Conflict,
        ErrorType.VALIDATION => StatusCodes.Status400BadRequest,
        ErrorType.FAILURE => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
}