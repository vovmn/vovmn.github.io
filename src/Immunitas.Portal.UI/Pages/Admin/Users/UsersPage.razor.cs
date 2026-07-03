using Immunitas.Portal.UI.ApiProxies.ApiServices.Laboratories;
using Immunitas.Portal.UI.ApiProxies.ApiServices.Users;
using Immunitas.Portal.UI.ApiProxies.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Immunitas.Portal.UI.Pages.Admin.Users;

public partial class UsersPage(
    IUsersApiService usersApiService,
    ILaboratoresApiService laboratoresApiService) : ComponentBase
{
    private MudDataGrid<UserDto>? _usersDataGrid;
    private string? _searchString;
    private bool _isLoading;

    private CreateUserDialog? _createUserDialog;
    private IEnumerable<LaboratoryDto> _laboratories = [];

    private async Task<GridData<UserDto>> ServerReload(GridState<UserDto> state)
    {
        try
        {
            _isLoading = true;
            var data = await usersApiService.GetUsers(new GetUsersRequest
            {
                Page = state.Page + 1,
                Count = state.PageSize,
                SearchText = _searchString,
                OrderBy = "FullName"
            });
            var laboratoriesResponse = await laboratoresApiService.GetLaboratories(new GetLaboratoriesRequest()
            {
                Page = 1,
                Count = int.MaxValue
            });
            _laboratories = laboratoriesResponse.Laboratories;

            return new GridData<UserDto>
            {
                TotalItems = data.Total,
                Items = data.Users
            };
        }
        finally
        {
            _isLoading = false;
        }
    }

    private Task OnSearch(string text)
    {
        _searchString = text;
        return _usersDataGrid?.ReloadServerData() ?? Task.CompletedTask;
    }
	private void OpenCreateUserDialog()
	{
		_createUserDialog?.OpenDialog();
	}

    private async Task OnUserCreated(CreateUserRequest request)
    {
        await usersApiService.CreateUser(request);
        await _usersDataGrid!.ReloadServerData();
    }
}
