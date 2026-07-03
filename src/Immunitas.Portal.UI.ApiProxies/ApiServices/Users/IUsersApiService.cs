using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Users;

public interface IUsersApiService : IApiService
{
    Task<GetUsersResponse> GetUsers(GetUsersRequest request, CancellationToken cancellationToken = default);
    Task CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default);
}
