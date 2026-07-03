using Immunitas.Domain.Entities.Patients;
using Immunitas.Generators.Attributes;

namespace Immunitas.Application.DTOs;

/// <summary>
/// Объект для передачи данных о пациенте
/// </summary>
[SharedContract]
public class PatientDto
{
    /// <summary>
    /// Id пациента
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// ФИО
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Пол пациента (мужской, женский, другой)
    /// </summary>
    public required Gender Gender { get; set; }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public required DateOnly BirthDate { get; set; }
}
