using Immunitas.Domain.Entities.Drugs;
using Immunitas.Domain.Entities.Measurements;
using Immunitas.Domain.Entities.Patients;

namespace Immunitas.Domain.Entities.Samples;

/// <summary>
/// Образец крови пациента, который будет использоваться для проведения анализа
/// </summary>
public class Sample : EntityBase
{
    /// <summary>
    /// Код пробирки с образцом крови пациента
    /// </summary>
    public required string Barcode { get; set; }

    /// <summary>
    /// Id пациента, которому принадлежит образец крови
    /// </summary>
    public required int PatientId { get; set; }

    /// <summary>
    /// Пациент, которому принадлежит образец крови
    /// </summary>
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Дата и время сбора образца крови у пациента
    /// </summary>
    public required DateTime CollectedAt { get; set; }

    /// <summary>
    /// Набор измерений, полученных в результате анализа образца крови пациента на проточном цитометре
    /// </summary>
    public ICollection<CytometerMeasurement> CytometerMeasurements { get; set; } = [];

    /// <summary>
    /// Информация о лекарственных средствах, которые принимал пациент на момент сбора образца крови
    /// </summary>
    public SampleDrugs[] SampleDrugs { get; set; } = [];
}