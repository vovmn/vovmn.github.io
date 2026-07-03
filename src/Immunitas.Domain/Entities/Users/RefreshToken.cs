namespace Immunitas.Domain.Entities.Users;

/// <summary>
/// Модель для хранения информации о refresh-токене, который используется для обновления access-токена без повторной аутентификации пользователя.
/// </summary>
public class RefreshToken : EntityBase
{
    /// <summary>
    /// Токен
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Дата/время окончания срока действия токена
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Дата/время генерации токена
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата/время отзыва токена.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Id пользователя, которому принадлежит токен
    /// </summary>
    public required int UserId { get; init; }

    /// <summary>
    /// Пользователь, которому принадлежит токен
    /// </summary>
    public User User { get; init; } = null!;

    /// <summary>
    /// Был ли токен уже использован для получения нового access-токена
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Возвращает true, если токен истек (текущая дата/время больше или равно дате окончания срока действия), иначе false
    /// </summary>
    /// <param name="now">Текущая дата/время для проверки истечения срока действия токена</param>
    /// <returns>True, если токен истек, иначе false</returns>
    public bool IsExpired(DateTime now) => now >= ExpiresAt;

    /// <summary>
    /// Возвращает true, если токен активен (не отозван и не истек), иначе false
    /// </summary>
    /// <param name="now">Текущая дата/время для проверки активности токена</param>
    /// <returns>True, если токен активен, иначе false</returns>
    public bool IsActive(DateTime now) => RevokedAt == null && !IsExpired(now);
}