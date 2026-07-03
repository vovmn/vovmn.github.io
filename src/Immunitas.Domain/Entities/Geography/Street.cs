namespace Immunitas.Domain.Entities.Geography;


/// <summary>
/// Улица
/// </summary>
public class Street : EntityBase
{
    /// <summary>
    /// Название улицы
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Город, в которой расположена улица
    /// </summary>
    public required int CityId { get; set; }

    /// <summary>
    /// Город, в котором расположена улица
    /// </summary>
    public City City { get; set; } = null!;
}
