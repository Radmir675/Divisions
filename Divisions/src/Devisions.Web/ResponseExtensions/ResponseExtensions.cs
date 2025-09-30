using Microsoft.AspNetCore.Mvc;
using Shared.Failures;

namespace Devisions.Web.ResponseExtensions;

public static class ResponseExtensions
{
    public static ActionResult ToResponse(this Failure failure)
    {
        if (!failure.Any())
            return new ObjectResult(null) { StatusCode = StatusCodes.Status500InternalServerError };

        var distinctErrors = failure.Select(x => x.ErrorType).Distinct().ToList();

        int statusCode = distinctErrors.Count() > 1
            ? StatusCodes.Status500InternalServerError
            : GetStatusCodeFromErrorType(distinctErrors[0]);

        return new ObjectResult(statusCode) { StatusCode = statusCode };
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