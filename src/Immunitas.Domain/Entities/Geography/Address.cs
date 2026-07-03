namespace Immunitas.Domain.Entities.Geography;

/// <summary>
/// Адрес, на котором расположен объект (например, пациент, медицинское учреждение и т.д.)
/// </summary>
public class Address : EntityBase
{
    /// <summary>
    /// Id улицы, на которой расположен адрес
    /// </summary>
    public required int StreetId { get; set; }

    /// <summary>
    /// Улица, на которой расположен адрес
    /// </summary>
    public Street Street { get; set; } = null!;

    /// <summary>
    /// Дом
    /// </summary>
    public required string House { get; set; }

    /// <summary>
    /// Квартира (офис)
    /// </summary>
    public string? Apartment { get; set; }

    /// <summary>
    /// Почтовый индекс
    /// </summary>
    public string? PostalCode { get; set; }
}
