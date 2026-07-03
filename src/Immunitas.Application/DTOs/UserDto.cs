using Immunitas.Domain.Entities.Users;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

[SharedContract]
public class UserDto
{
    /// <summary>
    /// Id пользователя
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    public required string Email { get; set; }

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
    /// Название лаборатории, к которой относится пользователь
    /// </summary>
    public required string LaboratoryName { get; set; }
}
