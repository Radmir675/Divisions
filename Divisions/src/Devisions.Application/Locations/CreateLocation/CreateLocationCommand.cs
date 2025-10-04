using Devisions.Application.Abstractions;
using Devisions.Contracts.Locations;

namespace Devisions.Application.Locations.CreateLocation;

public record CreateLocationCommand(CreateLocationDto request) : ICommand;