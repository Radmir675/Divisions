using Devisions.Application.Database;
using Devisions.Infrastructure.Postgres.Database;

namespace Devisions.Web.Extensions;

public static class DbContextExtension
{
    public static void AddDbContext(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<AppDbContext>(_ =>
            new AppDbContext(builder.Configuration.GetConnectionString("DefaultConnection")!));

        builder.Services.AddScoped<IReadDbContext, AppDbContext>(_ =>
            new AppDbContext(builder.Configuration.GetConnectionString("DefaultConnection")!));
    }
}