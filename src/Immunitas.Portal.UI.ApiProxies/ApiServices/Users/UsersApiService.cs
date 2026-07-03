using Flurl.Http;
using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Users;

public class UsersApiService(IFlurlClient client) : IUsersApiService
{
    private readonly string controllerPath = "/api/Users";

    private IFlurlRequest FlurlRequest => client.Request(controllerPath);

    public Task CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        return FlurlRequest.PostJsonAsync(request, cancellationToken: cancellationToken);
    }

    public Task<GetUsersResponse> GetUsers(GetUsersRequest request, CancellationToken cancellationToken = default)
    {
        return FlurlRequest
            .SetQueryParams(request)
            .GetJsonAsync<GetUsersResponse>(cancellationToken: cancellationToken);
    }
}
