namespace Devisions.Infrastructure.Postgres.Seeder;

public interface ISeeder
{
    Task CreateAsync(CancellationToken cancellationToken);
}