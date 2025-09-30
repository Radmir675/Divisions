using FluentValidation.Results;
using Shared.Failures;

namespace Devisions.Application.Extensions;

public static class ValidationExtensions
{
    public static Failure ToErrors(this ValidationResult validationResult) =>
        validationResult.Errors
            .Select(x => Error.Validation(x.ErrorCode, x.ErrorMessage, x.PropertyName))
            .ToArray();
}