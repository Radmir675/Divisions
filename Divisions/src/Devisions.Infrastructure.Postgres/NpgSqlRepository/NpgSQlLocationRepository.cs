using CSharpFunctionalExtensions;
using Devisions.Application.Locations;
using Devisions.Domain.Location;
using Shared.Errors;

namespace Devisions.Infrastructure.Postgres.NpgSqlRepository;

public class NpgSQlLocationRepository : ILocationRepository
{
    private readonly NpgSqlConnectionFactory _connectionFactory;

    public NpgSQlLocationRepository(NpgSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public async Task<Result<bool, Error>> ExistsByIdsAsync(
        IEnumerable<LocationId> locationsId,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.GetConnectionAsync();

        // const string sqlQuery= ""SELECT* FROM  """;
        // connection.ExecuteAsync()
        return null;
    }

    public Task<Result<IEnumerable<Location>, Error>> GetByIdsAsync(IEnumerable<LocationId> locationsId,
        CancellationToken cancellationToken) => throw new NotImplementedException();
}