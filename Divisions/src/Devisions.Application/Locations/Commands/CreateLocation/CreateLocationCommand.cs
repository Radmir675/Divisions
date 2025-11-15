using Devisions.Application.Abstractions;
using Devisions.Contracts.Locations.Requests;

namespace Devisions.Application.Locations.Commands.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;