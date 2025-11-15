namespace Devisions.Contracts.Locations.Requests;

public record GetLocationsRequest(
    IEnumerable<Guid>? DepartmentIds,
    string? Search,
    bool? IsActive,
    int? Page = 1,
    int? PageSize = 20
);