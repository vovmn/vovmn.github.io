using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Users;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler(
    IRepository<User> usersRepository) : IGetUsersQueryHandler
{
    public async Task<GetUsersQueryResult> Handle(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        // Получаем список пациентов
        var users = usersRepository.AsQueryable();

        // Фильтруем по ФИО
        if (!string.IsNullOrWhiteSpace(query.SearchText))
            users = ApplyFilter(users, query.SearchText);

        // Осуществляем пагинацию и загружаем данные в память
        var usersList = await users
            .OrderBy(u => u.FullName)
            .Skip((query.Page - 1) * query.Count)
            .Take(query.Count)
            .Select(u => new UserDto
            {
                CreatedAt = u.CreatedAt,
                Email = u.Email,
                FullName = u.FullName,
                Id = u.Id,
                LaboratoryName = u.Laboratory.Name,
                Role = u.Role,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        // Получаем общее количество пациентов
        var total = await usersRepository.CountAsync(cancellationToken);

        // Возвращаем результат
        return new GetUsersQueryResult
        {
            Users = usersList,
            Total = total
        };
    }

    /// <summary>
    /// Осуществляет фильтрацию по ФИО и E-mail
    /// </summary>
    /// <param name="patients">Запрос пациентов</param>
    /// <param name="search">ФИО или E-mail</param>
    /// <returns>Запрос с пользователями, отфильтрованными по ФИО и E-mail</returns>
    private IQueryable<User> ApplyFilter(
        IQueryable<User> patients,
        string search)
    {
        return int.TryParse(search, out var userId) 
            ? patients.Where(p => p.Id == userId)
            : patients.Where(p =>
                EF.Functions.ILike(p.FullName, $"%{search}%")
                || EF.Functions.ILike(p.Email, $"%{search}%"));
    }
}
