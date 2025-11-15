using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Database;
using Devisions.Application.Extensions;
using Devisions.Contracts.Locations.Requests;
using Devisions.Contracts.Locations.Responses;
using Devisions.Domain.Department;
using Devisions.Domain.Location;
using FluentValidation;
using Shared.Errors;

namespace Devisions.Application.Locations.Queries.GetLocation;

public record GetLocationQuery(GetLocationsRequest Request) : IQuery;

public class GetLocationsHandler : IQueryHandler<IEnumerable<LocationDto>, GetLocationQuery>
{
    private readonly IValidator<GetLocationQuery> _validator;
    private readonly IReadDbContext _readDbContext;

    public GetLocationsHandler(
        IValidator<GetLocationQuery> validator,
        IReadDbContext readDbContext)
    {
        _validator = validator;
        _readDbContext = readDbContext;
    }

    public async Task<Result<IEnumerable<LocationDto>, Errors>> Handle(
        GetLocationQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        IQueryable<Location> resultQuery = _readDbContext.LocationsRead;

        var departmentIds = query.Request.DepartmentIds?.Select(x => new DepartmentId(x)).ToList();
        if (departmentIds?.Any() == true)
        {
            resultQuery = resultQuery.Where(l =>
                _readDbContext.DepartmentsRead
                    .Where(d => departmentIds.Contains(d.Id))
                    .SelectMany(d => d.DepartmentLocations.Select(dl => dl.LocationId))
                    .Contains(l.Id));
        }

        if (!string.IsNullOrEmpty(query.Request.Search))
        {
            var searchTerm = query.Request.Search.ToLower();
            resultQuery = resultQuery.Where(x => x.Name.ToLower().Contains(searchTerm));
        }

        if (query.Request.IsActive.HasValue)
        {
            resultQuery = resultQuery.Where(l => l.IsActive == query.Request.IsActive.Value);
        }

        if (query.Request.Page.HasValue && query.Request.PageSize.HasValue)
        {
            var pageSize = query.Request.PageSize.Value;
            var page = query.Request.Page.Value;

            resultQuery = resultQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }

        resultQuery = resultQuery
            .OrderBy(x => x.Name)
            .ThenBy(x => x.CreatedAt);

        var locations = resultQuery.Select(l => new LocationDto()
        {
            Id = l.Id.Value,
            Name = l.Name,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            IsActive = l.IsActive,
            Timezone = l.Timezone.IanaTimeZone,
            Address = new AddressDto()
            {
                City = l.Address.City,
                Country = l.Address.Country,
                Street = l.Address.Street,
                HouseNumber = l.Address.HouseNumber,
                RoomNumber = l.Address.RoomNumber,
            },
        }).ToList();
        return locations;
    }
}