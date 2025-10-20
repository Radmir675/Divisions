using System.Reflection;
using CSharpFunctionalExtensions;
using Devisions.Web.Response;
using Microsoft.AspNetCore.Http.Metadata;
using Shared.Errors;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Devisions.Web.EndPointResults;

public class EndPointResult<TValue> : IResult, IEndpointMetadataProvider
{
    private readonly IResult _result;

    public EndPointResult(Result<TValue, Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorsResult(result.Error);
    }

    public EndPointResult(Result<TValue, Errors> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorsResult(result.Error);
    }

    public EndPointResult(UnitResult<Errors> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult()
            : new ErrorsResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext) =>
        _result.ExecuteAsync(httpContext);

    public static implicit operator EndPointResult<TValue>(Result<TValue, Error> result) => new(result);

    public static implicit operator EndPointResult<TValue>(Result<TValue, Errors> result) => new(result);

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Metadata.Add(new ProducesResponseTypeMetadata(200, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(500, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(400, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(404, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(401, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(403, typeof(Envelope<TValue>), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(409, typeof(Envelope<TValue>), ["application/json"]));
    }
}

public class EndPointResult : IResult, IEndpointMetadataProvider
{
    private readonly IResult _result;

    public EndPointResult(UnitResult<Errors> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult()
            : new ErrorsResult(result.Error);
    }

    public EndPointResult(UnitResult<Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult()
            : new ErrorsResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext) =>
        _result.ExecuteAsync(httpContext);

    public static implicit operator EndPointResult(UnitResult<Error> result) => new(result);

    public static implicit operator EndPointResult(UnitResult<Errors> result) => new(result);

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        builder.Metadata.Add(new ProducesResponseTypeMetadata(200, typeof(Envelope), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(500, typeof(Envelope), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(400, typeof(Envelope), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(404, typeof(Envelope), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(401, typeof(Envelope), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(403, typeof(Envelope), ["application/json"]));
        builder.Metadata.Add(new ProducesResponseTypeMetadata(409, typeof(Envelope), ["application/json"]));
    }
}