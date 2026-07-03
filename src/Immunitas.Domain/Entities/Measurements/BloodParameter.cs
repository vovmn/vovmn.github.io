namespace Immunitas.Domain.Entities.Measurements;

/// <summary>
/// Сущность, представляющая параметры, полученные в результате анализа образца крови пациента на проточном цитометре
/// </summary>
public class BloodParameter : EntityBase
{
    /// <summary>
    /// Id измерения, к которому относятся эти параметры
    /// </summary>
    public int CytometerMeasurementId { get; set; }
    
    /// <summary>
    /// Название параметра
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Значение параметра
    /// </summary>
    public required double Value { get; set; }
}
