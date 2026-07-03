using Immunitas.Portal.UI.ApiProxies.Contracts;
using Immunitas.Portal.UI.ApiProxies.Contracts.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Immunitas.Portal.UI.Pages.Admin.Users;

public partial class CreateUserDialog : ComponentBase
{
    [Parameter]
    public EventCallback<CreateUserRequest> OnUserCreated { get; set; }

    [Parameter]
    public IEnumerable<LaboratoryDto> Laboratories { get; set; } = [];

    private bool _isLoading = false;
    private bool _isDialogOpened = false;
    private bool _isValid;
    private DialogOptions _options = new()
    {
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
        CloseButton = true,
    };

    private string? _email { get; set; }
    private string? _password { get; set; }
    private string? _fullName { get; set; }
    private UserRole? _role { get; set; }
    private int? _laboratoryId { get; set; }

    public void OpenDialog() => OpenedChanged(true);

    public void CloseDialog() => OpenedChanged(false);

    private void OpenedChanged(bool opened)
    {
        if (!opened)
        {
            _email = null;
            _password = null;
            _fullName = null;
            _role = null;
            _laboratoryId = null;
        }
        _isDialogOpened = opened;
    }

    private async Task CreateUser()
    {
        if (string.IsNullOrWhiteSpace(_email) ||
            string.IsNullOrWhiteSpace(_password) ||
            string.IsNullOrWhiteSpace(_fullName) ||
            !_role.HasValue ||
            !_laboratoryId.HasValue)
        {
            return;
        }
        _isLoading = true;
        try
        {
            var request = new CreateUserRequest
            {
                Email = _email,
                Password = _password,
                FullName = _fullName,
                Role = _role.Value,
                LaboratoryId = _laboratoryId.Value
            };
            await OnUserCreated.InvokeAsync(request);
            CloseDialog();
        }
        finally
        {
            _isLoading = false;
        }
    }
}
