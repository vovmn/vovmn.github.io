using Immunitas.Domain.Entities.Laboratories;

namespace Immunitas.Domain.Entities.Users;

/// <summary>
/// Пользователь системы
/// </summary>
public class User : EntityBase
{
    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Хэш пароля пользователя
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Полное имя пользователя
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Роль пользователя в системе
    /// </summary>
    public required UserRole Role { get; set; }

    /// <summary>
    /// Дата и время создания пользователя
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата и время последнего обновления данных пользователя
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Id лаборатории, к которой привязан пользователь
    /// </summary>
    public required int LaboratoryId { get; set; }
    
    /// <summary>
    /// Лаборатория, к которой привязан пользователь
    /// </summary>
    public Laboratory Laboratory { get; set; } = null!;

    /// <summary>
    /// Пользователь удален?
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Маркер безопасности для управления сессиями и токенами. Генерируется при создании пользователя и обновляется при изменении пароля / выходе с устройства.
    /// </summary>
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Коллекция refresh-токенов, выданных пользователю
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
