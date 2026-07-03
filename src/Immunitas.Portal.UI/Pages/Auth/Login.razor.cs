using Immunitas.Portal.UI.ApiProxies.ApiServices.Auth;
using Immunitas.Portal.UI.ApiProxies.Contracts;
using Immunitas.Portal.UI.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Immunitas.Portal.UI.Pages.Auth;

public partial class Login(
    IAuthApiService authApiService,
    JwtAuthStateProvider jwtAuthStateProvider,
    IJSRuntime jsRuntime,
    NavigationManager navigationManager) : ComponentBase
{
    private string _email = string.Empty;
    private string _password = string.Empty;

    private bool _isFormValid;
    private bool _isLoginInvalid;
    private bool _isLoading;

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    private async Task HandleLogin()
    {
        try
        {
            _isLoading = true;
            _isLoginInvalid = false;
            var loginRequest = new LoginRequest
            {
                Email = _email,
                Password = _password
            };
            var loginResponse = await authApiService.Login(loginRequest);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", loginResponse.AccessToken);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", loginResponse.RefreshToken);
            jwtAuthStateProvider.NotifyUserChanged();
            navigationManager.NavigateTo(ReturnUrl ?? "/");
        }
        catch
        {
            _isLoginInvalid = true;
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}
