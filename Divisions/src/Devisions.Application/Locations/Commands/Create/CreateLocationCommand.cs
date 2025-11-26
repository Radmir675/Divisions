using Devisions.Application.Abstractions;
using Devisions.Contracts.Locations.Requests;

namespace Devisions.Application.Locations.Commands.Create;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;