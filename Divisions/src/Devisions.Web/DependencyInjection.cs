using Devisions.Application;
using Devisions.Infrastructure.Postgres;
using Devisions.Web.EndPointResults;
using Devisions.Web.Response;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            options.SchemaFilter<EndPointResultSchemaFilter>();
        });

        return services;
    }
}

public class EndPointResultSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsGenericType &&
            context.Type.GetGenericTypeDefinition() == typeof(EndPointResult<>))
        {
            var valueType = context.Type.GetGenericArguments()[0];

            // Полностью заменяем схему на схему Envelope<T>
            var envelopeType = typeof(Envelope<>).MakeGenericType(valueType);
            var envelopeSchema = context.SchemaGenerator.GenerateSchema(envelopeType, context.SchemaRepository);

            // Копируем свойства из envelopeSchema в текущую схему
            schema.Type = envelopeSchema.Type;
            schema.Properties = envelopeSchema.Properties;
            schema.Required = envelopeSchema.Required;
            schema.Description = "Standardized API response with envelope pattern";
        }
    }
}