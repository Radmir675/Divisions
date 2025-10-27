using CSharpFunctionalExtensions;
using Devisions.Application.Database;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.Database;

public class TransactionManager : ITransactionManager
{
    private readonly AppDbContext _dbContext;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<TransactionManager> _logger;

    public TransactionManager(AppDbContext dbContext, ILoggerFactory loggerFactory, ILogger<TransactionManager> logger)
    {
        _dbContext = dbContext;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
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