using System.Data;
using CSharpFunctionalExtensions;
using Devisions.Application.Transaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.Database;

public class TransactionManager : ITransactionManager
{
    private readonly AppDbContext _dbContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<TransactionManager> _logger;

    public ChangeTracker ChangeTracker { get; }

    public TransactionManager(AppDbContext dbContext, ILoggerFactory loggerFactory, ILogger<TransactionManager> logger)
    {
        _dbContext = dbContext;
        _loggerFactory = loggerFactory;
        _logger = logger;
        ChangeTracker = dbContext.ChangeTracker;
    }

    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken = default,
        IsolationLevel? isolationLevel = null)
    {
        try
        {
            var transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    isolationLevel ?? IsolationLevel.ReadCommitted,
                    cancellationToken);

            var logger = _loggerFactory.CreateLogger<TransactionScope>();
            var transactionScope = new TransactionScope(transaction.GetDbTransaction(), logger);
            return transactionScope;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to begin transaction");
            return Error.Failure("begin.transaction.failure", "transaction is not started");
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes");
            return Error.Failure("save.changes.async", "Failed save changes");
        }
    }
}