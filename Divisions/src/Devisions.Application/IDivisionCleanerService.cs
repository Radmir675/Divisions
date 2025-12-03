using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Devisions.Application;

public interface IDivisionCleanerService
{
    Task<UnitResult<Error>> Process(CancellationToken cancellationToken);
}