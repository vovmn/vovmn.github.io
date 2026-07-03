using Immunitas.Domain.Entities.Samples;

namespace Immunitas.Domain.Entities.Drugs;

/// <summary>
/// Связь между образцом крови пациента и лекарственным средством, которое принимал пациент на момент сбора образца крови
/// </summary>
public class SampleDrugs : EntityBase
{
    /// <summary>
    /// Id образца крови пациента
    /// </summary>
    public required int SampleId { get; set; }

    /// <summary>
    /// Образец крови пациента
    /// </summary>
    public Sample Sample { get; set; } = null!;

    /// <summary>
    /// Id лекарственного средства, которое принимал пациент на момент сбора образца крови
    /// </summary>
    public required int MedicalDrugId { get; set; }

    /// <summary>
    /// Лекарственное средство, которое принимал пациент на момент сбора образца крови
    /// </summary>
    public MedicalDrug MedicalDrug { get; set; } = null!;

    /// <summary>
    /// Указанная пациентом дозировка лекарственного средства, которое он принимал на момент сбора образца крови
    /// </summary>
    public string? Dosage { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }
}
