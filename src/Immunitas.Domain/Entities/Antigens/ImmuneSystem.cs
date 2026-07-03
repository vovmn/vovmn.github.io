using System;

namespace Immunitas.Domain.Entities.Antigens;

/// <summary>
/// Сущность, представляющая иммунную систему
/// </summary>
public class ImmuneSystem : EntityBase
{
    /// <summary>
    /// Название иммунной системы
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Дата и время создания иммунной системы
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Коллекция органов, относящихся к иммунной системе
    /// </summary>
    public ICollection<Organ> Organs { get; init; } = [];
}
