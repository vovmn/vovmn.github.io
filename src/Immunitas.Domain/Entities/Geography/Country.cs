namespace Immunitas.Domain.Entities.Geography;

/// <summary>
/// Страна
/// </summary>
public class Country : EntityBase
{
    /// <summary>
    /// Название страны
    /// </summary>
    public required string Name { get; set; }
}
