namespace Immunitas.Application.Users.Queries.GetUsers;

public interface IGetUsersQueryHandler : IHandler
{
    /// <summary>
    /// Возвращает список пользователей по запросу
    /// </summary>
    /// <param name="query">Запрос на получение пользователей</param>
    /// <returns>Объект со списком пользователей системы</returns>
    Task<GetUsersQueryResult> Handle(GetUsersQuery query, CancellationToken cancellationToken = default);
}
