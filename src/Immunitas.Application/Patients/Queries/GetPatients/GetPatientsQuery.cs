using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Patients.Queries.GetPatients
{
    /// <summary>
    /// Запрос на получения списка пациентов
    /// </summary>
    [SharedContract]
    public class GetPatientsQuery
    {
        /// <summary>
        /// Номер страницы
        /// </summary>
        public int Page { get; init; } = 1;

        /// <summary>
        /// Количество пациентов
        /// </summary>
        public int Count { get; init; } = 10;

        /// <summary>
        /// Текст для поиска по ФИО
        /// </summary>
        public string? SearchText { get; init; }

        /// <summary>
        /// Поле, по которому осущствлять сортировку
        /// </summary>
        public string? OrderBy { get; init; }
    }
}
