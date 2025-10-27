using Devisions.Infrastructure.Postgres;
using Devisions.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;

namespace Devisions.Web.Extensions;

public static class MigrateExtension
{
    public static async Task AddMigrateAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}