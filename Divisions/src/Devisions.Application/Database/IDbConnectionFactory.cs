using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Devisions.Application.Database;

public interface IDbConnectionFactory
{
    Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken);
}