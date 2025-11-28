using System.Threading;
using System.Threading.Tasks;

namespace Devisions.Application;

public interface IDivisionCleanerService
{
    Task Process(CancellationToken cancellationToken);
}