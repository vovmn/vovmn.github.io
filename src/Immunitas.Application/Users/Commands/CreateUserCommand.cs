using Immunitas.Domain.Entities.Users;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Users.Commands;

[SharedContract]
public class CreateUserCommand
{
    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Полное имя пользователя
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Роль пользователя в системе
    /// </summary>
    public required UserRole Role { get; set; }

    /// <summary>
    /// Id лаборатории, к которой привязан пользователь
    /// </summary>
    public required int LaboratoryId { get; set; }
}
