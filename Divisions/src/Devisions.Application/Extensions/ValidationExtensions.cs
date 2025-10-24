using System.Linq;
using FluentValidation.Results;
using Shared.Errors;

namespace Devisions.Application.Extensions;

public static class ValidationExtensions
{
    public static Errors ToErrors(this ValidationResult validationResult) =>
        validationResult.Errors
            .Select(x =>
            {
                string? errorMessage = x.ErrorMessage;
                var error = Error.Deserialize(errorMessage);
                return Error.Validation(error.Code, error.Message, x.PropertyName);
            })
            .ToArray();
}