using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Laboratories;

public interface ILaboratoresApiService : IApiService
{
    Task<GetLaboratoriesResult> GetLaboratories(GetLaboratoriesRequest request, CancellationToken cancellationToken = default);
}
