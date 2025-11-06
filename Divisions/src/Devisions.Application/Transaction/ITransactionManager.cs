using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Errors;

namespace Devisions.Application.Transaction;

public interface ITransactionManager
{
    public ChangeTracker ChangeTracker { get; }

    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken = default,
        IsolationLevel? isolationLevel = null);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}