using System.Text.Json;
using Shared.Errors;

namespace Devisions.Application.Exceptions;

public class BadRequestException(Error[] errors) : Exception(JsonSerializer.Serialize(errors));