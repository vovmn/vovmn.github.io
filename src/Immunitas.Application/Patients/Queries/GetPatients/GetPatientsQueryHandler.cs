using Immunitas.Application.DTOs;
using Immunitas.Domain.Entities.Patients;
using Immunitas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Immunitas.Application.Patients.Queries.GetPatients
{
    /// <summary>
    /// Обработчик запроса на получение пациентов
    /// </summary>
    /// <param name="patientsRepository">Репозиторий пациентов</param>
    public class GetPatientsQueryHandler(
        IRepository<Patient> patientsRepository) : IGetPatientsQueryHandler
    {
        /// <summary>
        /// Возвращает пагинированый список пациентов
        /// </summary>
        /// <param name="query">Запрос с входными данными на получение пациентов</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Таска с результатом выполнения запроса</returns>
        public async Task<GetPatientsQueryResult> Handle(GetPatientsQuery query, CancellationToken cancellationToken)
        {
            // Получаем список пациентов
            var patients = patientsRepository.AsQueryable();

            // Фильтруем по ФИО
            if (!string.IsNullOrWhiteSpace(query.SearchText))
                patients = ApplyFilterByFullName(patients, query.SearchText);

            // Осуществляем пагинацию и загружаем данные в память
            var patientsList = await patients
                .OrderBy(p => p.LastName)
                .Skip((query.Page - 1) * query.Count)
                .Take(query.Count)
                .Select(p => new PatientDto
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    Gender = p.Gender,
                    BirthDate = p.BirthDate
                })
                .ToListAsync(cancellationToken);

            // Получаем общее количество пациентов
            var total = await patientsRepository.CountAsync(cancellationToken);

            // Возвращаем результат
            return new GetPatientsQueryResult
            {
                Patients = patientsList,
                Total = total
            };
        }

        /// <summary>
        /// Осуществляет фильтрацию по ФИО
        /// </summary>
        /// <param name="patients">Запрос пациентов</param>
        /// <param name="fullName">ФИО</param>
        /// <returns>Запрос с пациентами, отфильтрованными по ФИО</returns>
        private IQueryable<Patient> ApplyFilterByFullName(
            IQueryable<Patient> patients, 
            string fullName)
        {
            return patients.Where(p =>
                    EF.Functions.ILike(p.MiddleName == null
                        ? p.LastName + " " + p.FirstName
                        : p.FirstName + " " + p.LastName + " " + p.MiddleName, $"%{fullName}%"));
        }
    }
}
