using System;
using System.Text.Json;
using Shared.Errors;

namespace Devisions.Application.Exceptions;

public class NotFoundException(Error[] errors) : Exception(JsonSerializer.Serialize(errors));