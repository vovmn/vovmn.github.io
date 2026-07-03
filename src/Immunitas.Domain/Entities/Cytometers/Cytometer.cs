namespace Immunitas.Domain.Entities.Cytometers;

/// <summary>
/// Проточный цитометр, который используется для проведения анализа крови пациента
/// </summary>
public class Cytometer : EntityBase
{
    /// <summary>
    /// Название проточного цитометра
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Серийный номер проточного цитометра
    /// </summary>
    public required string SerialNumber { get; set; }

    /// <summary>
    /// Модель проточного цитометра
    /// </summary>
    public required string Model { get; set; }
}
