using Immunitas.Domain.Entities.Geography;

namespace Immunitas.Domain.Entities.Laboratories;

/// <summary>
/// Лаборатория, которая проводит анализы
/// </summary>
public class Laboratory : EntityBase
{
    /// <summary>
    /// Id адреса, на котором расположена лаборатория
    /// </summary>
    public required int AddressId { get; set; }

    /// <summary>
    /// Адрес, на котором расположена лаборатория
    /// </summary>
    public Address Address { get; set; } = null!;

    /// <summary>
    /// Название лаборатории
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Номер телефона лаборатории
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Сущность удалена?
    /// </summary>
    public bool IsDeleted { get; set; }
}
