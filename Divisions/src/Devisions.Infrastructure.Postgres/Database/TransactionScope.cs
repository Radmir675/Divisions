using System.Data;
using CSharpFunctionalExtensions;
using Devisions.Application.Transaction;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.Database;

public class TransactionScope : ITransactionScope
{
    private readonly IDbTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }

    public UnitResult<Error> Commit()
    {
        try
        {
            _transaction.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to commit transaction");
            return Error.Failure("transaction.commit.failure", "Failed to commit transaction");
        }

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            _transaction.Rollback();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed rollback transaction");
            return Error.Failure("transaction.rollback.failure", "Failed rollback transaction");
        }

        return UnitResult.Success<Error>();
    }

    public void Dispose() => _transaction.Dispose();
}