using Devisions.Application;
using Devisions.Infrastructure.Postgres;
using Devisions.Infrastructure.Postgres.BackgroundServices;
using Devisions.Infrastructure.Postgres.Seeder;
using Devisions.Web.EndPointResults;

namespace Devisions.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services)
    {
        services
            .AddWebDependencies()
            .AddApplication()
            .AddInfrastructure();

        return services;
    }

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<EndPointResultOperationFilter>();
        });
        services.AddScoped<ISeeder, DevisionsSeeder>();
        services.AddScoped<IDivisionCleanerService, DivisionCleanerService>();

        return services;
    }
}