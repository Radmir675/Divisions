namespace Devisions.Contracts.Locations.Responses;

public record GetLocationDto(IEnumerable<LocationDto> LocationDto, long TotalCount);