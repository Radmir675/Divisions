using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Application.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}