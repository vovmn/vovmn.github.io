using System;

namespace Immunitas.Domain.Entities.Antigens;

/// <summary>
/// Сущность, представляющая орган иммунной системы
/// </summary>
public class Organ : EntityBase
{
    /// <summary>
    /// Название органа
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Дата и время создания органа
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Id иммунной системы, к которой относится орган
    /// </summary>
    public required int ImmuneSystemId { get; init; }

    /// <summary>
    /// Иммунная система, к которой относится орган
    /// </summary>
    public ImmuneSystem ImmuneSystem { get; init; } = null!;

    /// <summary>
    /// Коллекция антигенов, относящихся к органу
    /// </summary>
    public ICollection<Antigen> Antigens { get; init; } = [];
}
