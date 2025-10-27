using System;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Application.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}