namespace Devisions.Contracts.Shared;

public record PaginationRequest(int? Page = 1, int? Size = 20);