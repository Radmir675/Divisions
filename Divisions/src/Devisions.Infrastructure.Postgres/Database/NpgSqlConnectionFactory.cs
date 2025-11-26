using System.Data;
using Devisions.Application.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Devisions.Infrastructure.Postgres.Database;

public class NpgSqlConnectionFactory : IDisposable, IAsyncDisposable, IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSourse;

    public NpgSqlConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseLoggerFactory(ConsoleDBLogger());
        _dataSourse = dataSourceBuilder.Build();
    }

    public NpgSqlConnectionFactory(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseLoggerFactory(ConsoleDBLogger());
        _dataSourse = dataSourceBuilder.Build();
    }

    public async Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        return await _dataSourse.OpenConnectionAsync(cancellationToken);
    }

    public void Dispose() => _dataSourse.Dispose();

    public async ValueTask DisposeAsync() => await _dataSourse.DisposeAsync();

    private ILoggerFactory ConsoleDBLogger() =>
        LoggerFactory.Create(builder => builder.AddConsole());
}