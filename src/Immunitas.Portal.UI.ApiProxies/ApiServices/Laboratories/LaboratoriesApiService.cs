using Flurl.Http;
using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Laboratories;

public class LaboratoriesApiService(IFlurlClient client) : ILaboratoresApiService
{
    private readonly string controllerPath = "/api/Laboratories";

    private IFlurlRequest FlurlRequest => client.Request(controllerPath);

    public Task<GetLaboratoriesResult> GetLaboratories(GetLaboratoriesRequest request, CancellationToken cancellationToken)
    {
        return FlurlRequest
            .SetQueryParams(request)
            .GetJsonAsync<GetLaboratoriesResult>(cancellationToken: cancellationToken);
    }
}
