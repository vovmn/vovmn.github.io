using Immunitas.Application.DTOs.DaData;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Immunitas.Application.Services.DaData;

/// <summary>
/// Реализация сервиса для работы с DaData API
/// </summary>
public class DaDataService(
    HttpClient httpClient) : IDaDataService
{
    /// <summary>
    /// Поиск адресов в DaData
    /// </summary>
    /// <returns>список адресов от DaData</returns>
    public async Task<DaDataDto?> SearchAddresses(string query, int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
                return null;

            var request = new { query, count };
            var response = await httpClient.PostAsJsonAsync(
                "suggestions/api/4_1/rs/suggest/address",
                request,
                cancellationToken);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<DaDataDto>(cancellationToken: cancellationToken);

            return result;
        }
        catch
        {
            return null;
        }
    }
}
