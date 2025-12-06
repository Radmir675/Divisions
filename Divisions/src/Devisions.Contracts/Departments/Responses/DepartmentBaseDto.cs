using Devisions.Contracts.Positions.Responses;

namespace Devisions.Contracts.Departments.Responses;

public sealed record DepartmentBaseDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Identifier { get; init; } = null!;

    public Guid? ParentId { get; init; }

    public string Path { get; init; } = null!;

    public short Depth { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public bool HasMoreChildren { get; init; }

    public IReadOnlyList<DepartmentBaseDto> Children { get; init; } = [];

    public IReadOnlyList<PositionInfoDto> Positions { get; init; } = [];
}