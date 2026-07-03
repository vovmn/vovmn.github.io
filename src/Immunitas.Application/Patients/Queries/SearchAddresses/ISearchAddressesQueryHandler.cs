namespace Immunitas.Application.Patients.Queries.SearchAddresses;

public interface ISearchAddressesQueryHandler : IHandler
{
    Task<SearchAddressesQueryResult> Handle(SearchAddressesQuery query, CancellationToken cancellationToken);
}
