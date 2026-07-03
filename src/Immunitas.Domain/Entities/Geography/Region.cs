namespace Immunitas.Domain.Entities.Geography;

/// <summary>
/// Регион (субъект)
/// </summary>
public class Region : EntityBase
{
    /// <summary>
    /// Название региона
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Id страны, в которой расположен регион
    /// </summary>
    public required int CountryId { get; set; }

    /// <summary>
    /// Страна, в которой расположен регион
    /// </summary>
    public Country Country { get; set; } = null!;

}
