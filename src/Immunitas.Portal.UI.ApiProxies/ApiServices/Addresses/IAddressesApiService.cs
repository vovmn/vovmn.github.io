using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Addresses;

public interface IAddressesApiService : IApiService 
{
    Task<SearchAddressesResponse> SearchAddresses(SearchAddressesRequest query, CancellationToken cancellationToken = default);
    Task<CreateAddressResponse> CreateAddress(CreateAddressRequest command, CancellationToken cancellationToken = default);
    Task<DaDataDto> SearchAddressesDaData(string query, int count = 10, CancellationToken cancellationToken = default);
}
