using Immunitas.Domain.Entities.Geography;
using Immunitas.Domain.Entities.Samples;

namespace Immunitas.Domain.Entities.Patients;

/// <summary>
/// Сущность, представляющая собой пациента
/// </summary>
public class Patient : EntityBase
{
    /// <summary>
    /// ФИО
    /// </summary>
    public string FullName => MiddleName == null 
        ? LastName + " " + FirstName
        : FirstName + " " + LastName + " " + MiddleName;

    /// <summary>
    /// Имя
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Фамилия
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Отчество
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Пол
    /// </summary>
    public required Gender Gender { get; set; }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Номер телефона
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Электронная почта
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Id адреса, на котором проживает пациент
    /// </summary>
    public required int AddressId { get; set; }

    /// <summary>
    /// Информация об адресе, на котором проживает пациент
    /// </summary>
    public Address Address { get; set; } = null!;

    /// <summary>
    /// Дата и время создания записи о пациенте
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата и время обновления записи о пациенте
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Образцы крови пациента, которые будут использоваться для проведения анализа
    /// </summary>
    public List<Sample> Samples { get; set; } = [];
}