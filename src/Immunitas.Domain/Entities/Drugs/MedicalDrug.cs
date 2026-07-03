namespace Immunitas.Domain.Entities.Drugs;

/// <summary>
/// Справочник лекарственных средств
/// </summary>
public class MedicalDrug : EntityBase
{
    /// <summary>
    /// Название лекарственного средства
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Действующее вещество
    /// </summary>
    public required string ActiveSubstance { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    public string? Description { get; set; }
}
