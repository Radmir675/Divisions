using System.Text.Json;
using System.Text.Json.Serialization;
using Devisions.Application.Exceptions;
using Shared.Errors;

namespace Devisions.Web.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(httpContext, e);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        _logger.LogError(exception, exception.Message);
        (int code, Error[]? errors) = exception switch
        {
            BadRequestException badRequestException => (StatusCodes.Status500InternalServerError,
                JsonSerializer.Deserialize<Error[]>(exception.Message)),

            NotFoundException notFoundException => (StatusCodes.Status404NotFound,
                JsonSerializer.Deserialize<Error[]>(exception.Message)),

            _ => (StatusCodes.Status500InternalServerError, [Error.Failure(null, "Something went wrong")])
        };
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = code;
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
        };
        await httpContext.Response.WriteAsJsonAsync(errors, options);
    }
}