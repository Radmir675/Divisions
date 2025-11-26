using Devisions.Application.Abstractions;
using Devisions.Contracts.Positions.Requests;

namespace Devisions.Application.Positions.Create;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;