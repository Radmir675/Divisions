using Devisions.Web.Response;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Devisions.Web.EndPointResults;

public sealed class EndPointResultOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var (isEndPointResult, valueType) = GetEndPointResultType(context.MethodInfo.ReturnType);

        if (isEndPointResult)
        {
            var envelopeType = valueType != null
                ? typeof(Envelope<>).MakeGenericType(valueType)
                : typeof(Envelope);

            var envelopeSchema = context.SchemaGenerator.GenerateSchema(envelopeType, context.SchemaRepository);
            UpdateAllResponses(operation, envelopeSchema);
        }
    }

    private static (bool isEndPointResult, Type? valueType) GetEndPointResultType(Type returnType)
    {
        if (returnType.IsGenericType)
        {
            var genericType = returnType.GetGenericTypeDefinition();
            var innerType = returnType.GetGenericArguments()[0];

            // Обработка Task<EndPointResult<TValue>>
            if (genericType == typeof(Task<>) && innerType.IsGenericType)
            {
                var innerGenericType = innerType.GetGenericTypeDefinition();
                if (innerGenericType == typeof(EndPointResult<>))
                    return (true, innerType.GetGenericArguments()[0]);
            }

            // Обработка Task<EndPointResult> (без generic)
            if (genericType == typeof(Task<>) && innerType == typeof(EndPointResult))
                return (true, null);

            // Обработка EndPointResult<TValue>
            if (genericType == typeof(EndPointResult<>))
                return (true, returnType.GetGenericArguments()[0]);
        }

        // Обработка EndPointResult (без generic)
        if (returnType == typeof(EndPointResult))
            return (true, null);

        return (false, null);
    }

    private static void UpdateAllResponses(OpenApiOperation operation, OpenApiSchema envelopeSchema)
    {
        var responses = new[] { 200, 400, 401, 403, 404, 409, 500 };

        foreach (var statusCode in responses)
        {
            var statusKey = statusCode.ToString();

            if (!operation.Responses.ContainsKey(statusKey))
            {
                operation.Responses[statusKey] = new OpenApiResponse { Description = GetStatusDescription(statusCode) };
            }

            operation.Responses[statusKey].Content ??= new Dictionary<string, OpenApiMediaType>();

            if (!operation.Responses[statusKey].Content.ContainsKey("application/json"))
            {
                operation.Responses[statusKey].Content["application/json"] = new OpenApiMediaType();
            }

            operation.Responses[statusKey].Content["application/json"].Schema = envelopeSchema;
        }
    }

    private static string GetStatusDescription(int statusCode) => statusCode switch
    {
        200 => "OK",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        500 => "Internal Server Error",
        _ => "Unknown"
    };
}