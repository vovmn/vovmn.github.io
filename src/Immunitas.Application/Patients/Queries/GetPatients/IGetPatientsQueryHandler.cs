namespace Immunitas.Application.Patients.Queries.GetPatients
{
    /// <summary>
    /// Интерфейс обработчика на получение списка пациентов
    /// </summary>
    public interface IGetPatientsQueryHandler : IHandler
    {
        /// <summary>
        /// Получить список пациентов
        /// </summary>
        /// <param name="query">Запрос с информацией о фильтрации, пагинации и сортировки</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Таска с результатом выполнения запроса</returns>
        Task<GetPatientsQueryResult> Handle(GetPatientsQuery query, CancellationToken cancellationToken);
    }
}
