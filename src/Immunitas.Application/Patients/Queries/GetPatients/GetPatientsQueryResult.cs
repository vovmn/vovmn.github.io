using Immunitas.Application.DTOs;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.Patients.Queries.GetPatients
{
    /// <summary>
    /// Список полученных пациентов и их общее количество
    /// </summary>
    [SharedContract]
    public class GetPatientsQueryResult
    {
        /// <summary>
        /// Список пациентов
        /// </summary>
        public required IReadOnlyList<PatientDto> Patients { get; set; } = [];

        /// <summary>
        /// Общее количество пациентов
        /// </summary>
        public required int Total { get; set; }
    }
}
