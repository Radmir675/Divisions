using System.Text.Json;
using Shared.Failures;

namespace Devisions.Application.Exceptions;

public class NotFoundException(Error[] errors) : Exception(JsonSerializer.Serialize(errors));