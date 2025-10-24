using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Domain.Position;
using Shared.Errors;

namespace Devisions.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken);

    Task<Result<bool, Error>> IsNameActiveAndFreeAsync(PositionName name, CancellationToken cancellationToken);
}