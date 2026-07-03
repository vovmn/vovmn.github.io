using Immunitas.Portal.UI.ApiProxies.ApiServices.Patients;
using Immunitas.Portal.UI.ApiProxies.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Immunitas.Portal.UI.Pages.Patients
{
    public partial class Patients(
        IPatientsApiService patientsApiService,
        NavigationManager navigationManager) : ComponentBase
    {
        private MudDataGrid<PatientDto>? _patientsDataGrid;
        private string? _searchString;
        private bool _isLoading;

        private async Task<GridData<PatientDto>> ServerReload(GridState<PatientDto> state)
        {
            try
            {
                _isLoading = true;
                var data = await patientsApiService.GetPatients(new GetPatientsRequest
                {
                    Page = state.Page + 1,
                    Count = state.PageSize,
                    SearchText = _searchString,
                    OrderBy = "FullName"
                });

                return new GridData<PatientDto>
                {
                    TotalItems = data.Total,
                    Items = data.Patients
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
            return _patientsDataGrid?.ReloadServerData() ?? Task.CompletedTask;
        }

        private void GoToCreatePatient()
        {
            navigationManager.NavigateTo($"/patients/create");
        }
    }
}
