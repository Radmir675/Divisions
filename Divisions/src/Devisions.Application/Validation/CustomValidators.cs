using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Validation;

public static class CustomValidators
{
    public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TValueObject, Error>> factoryMethod)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, Error> result = factoryMethod(value);
            if (result.IsSuccess)
                return;

            context.AddFailure(result.Error.Serialize());
        });
    }

    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, Error error)
    {
        return rule.WithErrorCode(error.Serialize());
    }

    public static IRuleBuilderOptionsConditions<T, IEnumerable<TItem>> MustHaveUniqueValues<T, TItem>(
        this IRuleBuilder<T, IEnumerable<TItem>> ruleBuilder,
        IEqualityComparer<TItem>? comparer = null)
    {
        return ruleBuilder.Custom((collection, context) =>
        {
            if (collection == null) return;

            var list = collection.ToList();

            if (list.Count == 0) return;

            if (list.Count != list.Distinct(comparer).Count())
            {
                // В зависимости от реализации Error
                context.AddFailure(GeneralErrors.NonUniqueValues().Serialize());
            }
        });
    }
}