using System;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Application.Transaction;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}