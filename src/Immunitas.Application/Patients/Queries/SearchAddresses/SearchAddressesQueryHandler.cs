using Immunitas.Domain.Entities.Geography;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Immunitas.Application.DTOs;

namespace Immunitas.Application.Patients.Queries.SearchAddresses;

public class SearchAddressesQueryHandler(
    IRepository<Address> addressRepository) : ISearchAddressesQueryHandler
{
    public async Task<SearchAddressesQueryResult> Handle(
        SearchAddressesQuery query,
        CancellationToken cancellationToken)
    {
        var addressesQuery = addressRepository
            .Include(a => a.Street)
                .ThenInclude(s => s.City)
                    .ThenInclude(s => s.Region)
                        .ThenInclude(c => c.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Region))
            addressesQuery = addressesQuery.Where(a =>
                EF.Functions.ILike(a.Street.City.Region.Name, $"%{query.Region}%"));

        if (!string.IsNullOrWhiteSpace(query.City))
            addressesQuery = addressesQuery.Where(a =>
                EF.Functions.ILike(a.Street.City.Name, $"%{query.City}%"));

        if (!string.IsNullOrWhiteSpace(query.Street))
            addressesQuery = addressesQuery.Where(a =>
                EF.Functions.ILike(a.Street.Name, $"%{query.Street}%"));

        if (!string.IsNullOrWhiteSpace(query.House))
            addressesQuery = addressesQuery.Where(a =>
                EF.Functions.ILike(a.House, $"%{query.House}%"));

        var addresses = await addressesQuery
            .Take(query.Take)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                Country = a.Street.City.Region.Country.Name,
                Region = a.Street.City.Region.Name,
                City = a.Street.City.Name,
                Street = a.Street.Name,
                House = a.House,
                Apartment = a.Apartment,
                PostalCode = a.PostalCode
            })
            .ToListAsync(cancellationToken);

        return new SearchAddressesQueryResult
        {
            Addresses = addresses
        };
    }
}
