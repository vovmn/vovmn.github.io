using Immunitas.Generators.Attributes;
using System.ComponentModel;

namespace Immunitas.Domain.Entities.Patients;

/// <summary>
/// Пол пациента
/// </summary>
[SharedContract]
public enum Gender
{
    /// <summary>
    /// Мужской
    /// </summary>
    [Description("Мужской")]
    Male = 1,

    /// <summary>
    /// Женский
    /// </summary>
    [Description("Женский")]
    Female = 2,

    /// <summary>
    /// Другой (либо не указан)
    /// </summary>
    [Description("Не указан")]
    Other = -1
}
