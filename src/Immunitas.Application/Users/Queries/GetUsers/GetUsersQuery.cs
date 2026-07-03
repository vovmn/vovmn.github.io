using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Users.Queries.GetUsers;

[SharedContract]
public class GetUsersQuery
{
    public int Page { get; set; } = 1;

    public int Count { get; set; } = 10;

    /// <summary>
    /// Текст для поиска по ФИО
    /// </summary>
    public string? SearchText { get; init; }

    /// <summary>
    /// Поле, по которому осущствлять сортировку
    /// </summary>
    public string? OrderBy { get; init; }
}
