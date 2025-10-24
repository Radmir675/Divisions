using Devisions.Application.Abstractions;
using Devisions.Contracts.Positions;

namespace Devisions.Application.Positions.CreatePositions;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;