using Flurl.Http;
using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Auth;

public class AuthApiService(IFlurlClient client) : IAuthApiService
{
    private readonly string controllerPath = "/api/Auth";

    private IFlurlRequest FlurlRequest => client.Request(controllerPath);

    public async Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await FlurlRequest
            .AppendPathSegment("/Login")
            .PostJsonAsync(request, cancellationToken: cancellationToken)
            .ReceiveJson<LoginResponse>();
        return response;
    }

    public async Task Logout(CancellationToken cancellationToken = default)
    {
        await FlurlRequest
            .AppendPathSegment("/Logout")
            .PostAsync(cancellationToken: cancellationToken);
    }

    public async Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var response = await FlurlRequest
            .AppendPathSegment("/RefreshToken")
            .PostJsonAsync(request, cancellationToken: cancellationToken)
            .ReceiveJson<RefreshTokenResponse>();
        return response;
    }
}
