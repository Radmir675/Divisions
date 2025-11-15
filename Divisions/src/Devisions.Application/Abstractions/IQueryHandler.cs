using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Application.Abstractions;

public interface IQuery;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse, Errors>> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery>
    where TQuery : IQuery
{
    Task<UnitResult<Errors>> Handle(TQuery query, CancellationToken cancellationToken);
}