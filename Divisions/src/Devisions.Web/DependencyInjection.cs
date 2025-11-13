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
            options.OperationFilter<EndPointResultOperationFilter>();
        });

        return services;
    }
}

public class EndPointResultOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var returnType = GetEndPointResultType(context.MethodInfo.ReturnType);
        if (returnType != null)
        {
            var valueType = returnType.GetGenericArguments()[0];
            var envelopeType = typeof(Envelope<>).MakeGenericType(valueType);
            var envelopeSchema = context.SchemaGenerator.GenerateSchema(envelopeType, context.SchemaRepository);

            UpdateSuccessResponses(operation, envelopeSchema);
        }
    }

    private static Type GetEndPointResultType(Type returnType)
    {
        if (returnType.IsGenericType)
        {
            var genericType = returnType.GetGenericTypeDefinition();
            var innerType = returnType.GetGenericArguments()[0];

            if (genericType == typeof(Task<>) && innerType.IsGenericType &&
                innerType.GetGenericTypeDefinition() == typeof(EndPointResult<>))
                return innerType;

            if (genericType == typeof(EndPointResult<>))
                return returnType;
        }

        return null;
    }

    private static void UpdateSuccessResponses(OpenApiOperation operation, OpenApiSchema envelopeSchema)
    {
        foreach (var response in operation.Responses.Where(r =>
                     int.TryParse(r.Key, out var code) && code >= 200 && code < 300))
        {
            if (response.Value.Content.ContainsKey("application/json"))
                response.Value.Content["application/json"].Schema = envelopeSchema;
        }
    }
}