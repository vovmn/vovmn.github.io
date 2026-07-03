using Immunitas.Application.DTOs.DaData;

namespace Immunitas.Application.Services.DaData;

/// <summary>
/// Сервис для работы с DaData API
/// </summary>
public interface IDaDataService
{
    /// <summary>
    /// Поиск адресов по строке
    /// </summary>
    /// <param name="query">Поисковый запрос</param>
    /// <param name="count">Количество результатов</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список найденных адресов</returns>
    Task<DaDataDto?> SearchAddresses(string query, int count = 10, CancellationToken cancellationToken = default);
}