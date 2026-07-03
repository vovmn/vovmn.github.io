using Immunitas.Application.DTOs;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Users.Queries.GetUsers;

[SharedContract]
public class GetUsersQueryResult
{
    /// <summary>
    /// Список объектов, содержащих информацию о пользователях
    /// </summary>
    public required IReadOnlyList<UserDto> Users { get; set; }

    /// <summary>
    /// Общее количество пользователей в системе
    /// </summary>
    public required int Total { get; set; }
}
