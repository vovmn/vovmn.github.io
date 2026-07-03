using Immunitas.Portal.UI.ApiProxies.Contracts;

namespace Immunitas.Portal.UI.ApiProxies.ApiServices.Auth;

/// <summary>
/// Интерфейс для сервиса взаимодействия с аутентификацией
/// </summary>
public interface IAuthApiService : IApiService
{
    /// <summary>
    /// Аутентификация пользователя и получение access и refresh токенов
    /// </summary>
    /// <param name="request">Запрос на аутентификацию пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Ответ с access и refresh токенами</returns>
    Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Запрашивает новый access токен, используя refresh токен.
    /// </summary>
    /// <param name="request">Запрос на обновление токена</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Ответ с новым access токеном</returns>
    Task<RefreshTokenResponse> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выход пользователя из системы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    Task Logout(CancellationToken cancellationToken = default);
}
