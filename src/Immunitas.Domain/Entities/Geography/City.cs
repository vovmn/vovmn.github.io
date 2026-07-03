namespace Immunitas.Domain.Entities.Geography;

/// <summary>
/// Город
/// </summary>
public class City : EntityBase
{
    /// <summary>
    /// Название города
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Id региона, в которой расположен город
    /// </summary>
    public required int RegionId { get; set; }

    /// <summary>
    /// Регион, в котором расположен город
    /// </summary>
    public Region Region { get; set; } = null!;
}
