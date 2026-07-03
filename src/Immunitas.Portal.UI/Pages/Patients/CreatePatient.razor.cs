using Immunitas.Portal.UI.ApiProxies.ApiServices.Patients;
using Immunitas.Portal.UI.ApiProxies.ApiServices.Addresses;
using Immunitas.Portal.UI.ApiProxies.Contracts;
using Immunitas.Portal.UI.ApiProxies.Contracts.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Immunitas.Portal.UI.Pages.Patients
{
    public partial class CreatePatient(
        IPatientsApiService patientsApiService,
        IAddressesApiService addressesApiService,
        NavigationManager navigationManager,
        ISnackbar snackbar) : ComponentBase
    {
        private CreatePatientRequest _command = new()
        {
            FirstName = "",
            LastName = "",
            MiddleName = "",
            Gender = Gender.Male,
            BirthDate = DateOnly.FromDateTime(DateTime.Today),
            Email = "",
            PhoneNumber = "",
            AddressId = 0
        };

        private bool _isValid;
        private bool _isSubmitting;
        private string[] _errors = [];
        private string? _errorMessage;
        private DateTime? _birthDate = DateTime.Today;
        private bool _isCreatingAddress;
        private DaDataSuggestion? _selectedSuggestion;

        private async Task HandleValidSubmit()
        {
            _isSubmitting = true;
            _errorMessage = null;

            try
            {
                if (_birthDate.HasValue)
                    _command.BirthDate = DateOnly.FromDateTime(_birthDate.Value);

                if (_selectedSuggestion != null)
                {
                    var data = _selectedSuggestion.Data;
                    var searchQuery = new SearchAddressesRequest
                    {
                        Country = data.Country ?? "",
                        Region = data.Region ?? "",
                        City = data.City ?? data.Settlement ?? "",
                        Street = data.Street ?? "",
                        House = data.House ?? "",
                        Take = 10
                    };

                    var searchResult = await addressesApiService.SearchAddresses(searchQuery);

                    if (!searchResult.Addresses.Any())
                    {
                        var createAddressCommand = new CreateAddressRequest
                        {
                            Country = data.Country ?? "",
                            Region = data.Region ?? "",
                            City = data.City ?? data.Settlement ?? "",
                            Street = data.Street ?? "",
                            House = data.House ?? "",
                            Apartment = data.Flat ?? "",
                            PostalCode = data.PostalCode
                        };

                        var createdAddress = await addressesApiService.CreateAddress(createAddressCommand);
                        _command.AddressId = createdAddress.Id;
                    }
                    else
                    {
                        var existingAddress = searchResult.Addresses.First();
                        _command.AddressId = existingAddress.Id;
                    }
                }
                var result = await patientsApiService.CreatePatient(_command);

                snackbar.Add($"Пациент успешно создан!", Severity.Success);
                await Task.Delay(1000);
                navigationManager.NavigateTo("/patients");
            }
            catch (Exception ex)
            {
                snackbar.Add($"Ошибка при создании пациента: {ex.Message}");
            }
            finally
            {
                _isSubmitting = false;
            }
        }

        private async Task<IEnumerable<DaDataSuggestion>> SearchAddress(string value, CancellationToken cancellationToken)
        {
            
            if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
                return [];

            try
            {
                var result = await addressesApiService.SearchAddressesDaData(value, 10, cancellationToken);
                return result.Suggestions;
            }
            catch (Exception ex)
            {
                snackbar.Add($"Ошибка при поиске адресов!", Severity.Error);
                return [];
            }
        }

        private async Task OnAddressSelected(DaDataSuggestion selectedSuggestion)
        {
            if (selectedSuggestion == null || _isCreatingAddress)
                return;

            _selectedSuggestion = selectedSuggestion;
        }

        private void Cancel()
        {
            navigationManager.NavigateTo("/patients");
        }
    }
}
