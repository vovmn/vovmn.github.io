using Immunitas.Portal.UI.ApiProxies.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Immunitas.Portal.UI.Auth;

public class AuthHandler(
    IJSRuntime jsRuntime,
    IServiceProvider serviceProvider,
    NavigationManager navigationManager) : DelegatingHandler
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Проверяем, не обновил ли кто-то токен, пока мы ждали семафор
                var currentToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

                // Если токен в хранилище уже сменился — значит, кто-то другой обновил его
                if (currentToken != token)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentToken);
                    return await base.SendAsync(request, cancellationToken);
                }

                // Если токен всё еще старый — обновляем
                var refreshed = await TryRefreshToken();
                if (refreshed)
                {
                    var newToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    return await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    navigationManager.NavigateTo("/login");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshToken()
    {
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("AuthAPI");

        var accessToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");
        var refreshToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.PostAsJsonAsync("api/Auth/RefreshToken", new { OldToken = refreshToken });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", result!.AccessToken);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", result.RefreshToken);
            return true;
        }

        return false;
    }
}