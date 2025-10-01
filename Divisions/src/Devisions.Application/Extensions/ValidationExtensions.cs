using FluentValidation.Results;
using Shared.Errors;

namespace Devisions.Application.Extensions;

public static class ValidationExtensions
{
    public static Errors ToErrors(this ValidationResult validationResult) =>
        validationResult.Errors
            .Select(x => Error.Validation(x.ErrorCode, x.ErrorMessage, x.PropertyName))
            .ToArray();
}