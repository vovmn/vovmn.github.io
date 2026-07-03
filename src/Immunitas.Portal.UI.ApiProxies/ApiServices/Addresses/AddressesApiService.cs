using Flurl.Http;
using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Addresses;

public class AddressesApiService(IFlurlClient client) : IAddressesApiService
{
    private readonly string _controllerPath = "/api/Addresses";
    private IFlurlRequest FlurlRequest => client.Request(_controllerPath);

    public async Task<SearchAddressesResponse> SearchAddresses(
        SearchAddressesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await FlurlRequest
            .AppendPathSegment("search")
            .SetQueryParams(request)
            .GetJsonAsync<SearchAddressesResponse>(cancellationToken:cancellationToken);
    }

    public async Task<CreateAddressResponse> CreateAddress(
        CreateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        return await FlurlRequest
            .PostJsonAsync(request, cancellationToken:cancellationToken)
            .ReceiveJson<CreateAddressResponse>();
    }

    public async Task<DaDataDto> SearchAddressesDaData(string query, int count = 10, CancellationToken cancellationToken = default)
    {
        return await FlurlRequest
            .AppendPathSegment("external/search")
            .SetQueryParams(new { query, count })
            .GetJsonAsync<DaDataDto>(cancellationToken:cancellationToken);
    }
}
