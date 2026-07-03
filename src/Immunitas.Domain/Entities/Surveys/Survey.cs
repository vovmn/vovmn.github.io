using Immunitas.Domain.Entities.Cytometers;
using Immunitas.Domain.Entities.Laboratories;
using Immunitas.Domain.Entities.Measurements;
using Immunitas.Domain.Entities.Patients;

namespace Immunitas.Domain.Entities.Surveys;

/// <summary>
/// Обследование пациента
/// </summary>
public class Survey : EntityBase
{
    /// <summary>
    /// Id лаборатории, в которой проводилось обследование пациента
    /// </summary>
    public required int LaboratoryId { get; init; }

    /// <summary>
    /// Лаборатория, в которой проводилось обследование пациента
    /// </summary>
    public Laboratory Laboratory { get; init; } = null!;

    /// <summary>
    /// Id пациента, которому принадлежит обследование
    /// </summary>
    public required int PatientId { get; init; }

    /// <summary>
    /// Пациент, которому принадлежит обследование
    /// </summary>
    public Patient Patient { get; init; } = null!;

    /// <summary>
    /// Дата и время создания обследования пациента
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время последнего обновления обследования пациента
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Коллекция измерений, проведенных в рамках обследования пациента
    /// </summary>
    public ICollection<CytometerMeasurement> CytometerMeasurements { get; init; } = [];

    /// <summary>
    /// Id проточного цитометра, на котором было проведено обследование пациента
    /// </summary>
    public required int CytometerId { get; init; }

    /// <summary>
    /// Цитометр, на котором было проведено обследование пациента
    /// </summary>
    public Cytometer Cytometer { get; init; } = null!;
}
