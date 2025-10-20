using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Devisions.Infrastructure.Postgres.NpgSqlRepository;

public class NpgSqlConnectionFactory : IDisposable, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSourse;

    public NpgSqlConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseLoggerFactory(ConsoleDBLogger());
        _dataSourse = dataSourceBuilder.Build();
    }

    public async Task<IDbConnection> GetConnectionAsync()
    {
        return await _dataSourse.OpenConnectionAsync();
    }

    private ILoggerFactory ConsoleDBLogger() =>
        LoggerFactory.Create(builder => builder.AddConsole());

    public void Dispose() => _dataSourse.Dispose();

    public async ValueTask DisposeAsync() => await _dataSourse.DisposeAsync();
}